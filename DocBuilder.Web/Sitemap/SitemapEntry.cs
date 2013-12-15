using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace DocBuilder.Web.Sitemap
{
    public class SitemapEntry
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public IList<SitemapEntry> Children { get; set; }

        public SitemapEntry()
        {
            Children = new List<SitemapEntry>();
        }

        public void Save(string ownerId, string siteId, string versionId, string format = "json")
        {
            var container = GetContainer();
            var path = string.Format("{0}/{1}/{2}/{3}", ownerId, siteId, versionId, format);

            var settings = new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(path);
            blockBlob.UploadText(JsonConvert.SerializeObject(this, settings));
        }

        public static SitemapEntry Get(string ownerId, string siteId, string versionId, string format = "json")
        {
            var container = GetContainer();
            var path = string.Format("{0}/{1}/{2}/{3}", ownerId, siteId, versionId, format);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(path);
            if (blockBlob == null || !blockBlob.Exists())
                return null;

            var settings = new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var sitemap = JsonConvert.DeserializeObject<SitemapEntry>(blockBlob.DownloadText(), settings);

            return sitemap;
        }

        public static CloudBlobContainer GetContainer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount; // CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference("sitemaps");

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            return container;
        }

    }
}
