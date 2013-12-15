using DocBuilder.Web.Sitemap;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocBuilder.Web.Models
{
    public class DocPage // : TableEntity
    {
        public string OwnerId { get; set; }
        public string SiteId { get; set; }

        public string VersionId { get; set; }

        public string PagePath { get; set; }

        public string Html { get; set; }

        //public string HtmContent { get; set; }

        //public PageMetadata Metadata { get; set; }

        //public DocPage(string ownerId, string siteId, string versionId, string pagePath)
        //{
        //    //PartitionKey = ownerId + "." + siteId;
        //    //RowKey = versionId;
        //}

        public DocPage() { }

        public void Save()
        {
            var container = GetContainer();

            string fullPath = string.Format("{0}/{1}/{2}/{3}", OwnerId, SiteId, VersionId, PagePath);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fullPath);
            blockBlob.UploadText(Html);
        }

        public static DocPage Get(string ownerId, string siteId, string versionId, string pagePath)
        {
            var container = GetContainer();

            string path = string.Format("{0}/{1}/{2}/{3}", ownerId, siteId, versionId, pagePath);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(path);

            if (blockBlob == null || !blockBlob.Exists())
                return null;

            var doc = new DocPage();
            doc.OwnerId = ownerId;
            doc.SiteId = siteId;
            doc.VersionId = versionId;
            doc.PagePath = pagePath;
            doc.Html = blockBlob.DownloadText();

            return doc;
        }

        public static SitemapEntry BuildSitemap(string ownerId, string siteId, string versionId)
        {
            var root = new SitemapEntry() {
                Name = "Root"
            };

            var path = string.Format("{0}/{1}/{2}/", ownerId, siteId, versionId);
            BuildSitemap(GetContainer(), root, path);
            return root;
        }

        static void BuildSitemap(CloudBlobContainer container, SitemapEntry parent, string path)
        {
            foreach (var item in container.ListBlobs(path, false)) {
                if (item.GetType() == typeof(CloudBlobDirectory)) {
                    var dir = (CloudBlobDirectory)item;
                    var entry = new SitemapEntry() {
                        Name = dir.Prefix.Substring(dir.Prefix.TrimEnd('/').LastIndexOf('/') + 1).TrimEnd('/'),// System.IO.Path.GetFileName(dir.Uri.AbsolutePath),
                        Url = "/docs/" + dir.Prefix + "home"
                    };
                    parent.Children.Add(entry);
                    BuildSitemap(container, entry, dir.Prefix);
                } else if (item.GetType() == typeof(CloudBlockBlob)) {
                    var doc = (CloudBlockBlob)item;
                    var entry = new SitemapEntry() {
                        Name = doc.Name.Substring(doc.Name.TrimEnd('/').LastIndexOf('/') + 1),
                        Url = "/docs/" + doc.Name
                    };
                    parent.Children.Add(entry);
                }
            }
        }

        //public static IEnumerable<SiteVersion> Get(string ownerId, string siteId)
        //{
        //    var table = GetTable();

        //    var query = new TableQuery<SiteVersion>().Where(
        //        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ownerId + "." + siteId));

        //    return table.ExecuteQuery(query);
        //}


        public static CloudBlobContainer GetContainer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount; // CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference("pages");

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            return container;
        }
    }
}