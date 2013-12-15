using DocBuilder.Web.Sitemap;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocBuilder.Web.Models
{
    public class Site : TableEntity
    {
        public string CurrentVersionId { get; set; }

        //public string HtmContent { get; set; }

        //public PageMetadata Metadata { get; set; }

        SitemapEntry _Sitemap;

        public SitemapEntry GetSitemap(string versionId)
        {
            if (_Sitemap == null)
                _Sitemap = SitemapEntry.Get(PartitionKey, RowKey, versionId ?? CurrentVersionId);
            return _Sitemap;
        }


        public Site(string ownerId, string siteId)
        {
            PartitionKey = ownerId;
            RowKey = siteId;
        }

        public Site() { }

        public void Save()
        {
            var table = GetTable();

            var op = TableOperation.InsertOrReplace(this);
            table.Execute(op);
        }

        public static Site Get(string ownerId, string siteId)
        {
            var table = GetTable();
            var op = TableOperation.Retrieve<Site>(ownerId, siteId);
            var retrievedResult = table.Execute(op);

            var site = (Site)retrievedResult.Result;

            return site;
        }

        public static IEnumerable<Site> Get(string ownerId)
        {
            var table = GetTable();

            var query = new TableQuery<Site>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ownerId));

            return table.ExecuteQuery(query);
        }


        public static CloudTable GetTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount; // CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=<your_storage_name>;AccountKey=<your_account_key>");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("Sites");
            table.CreateIfNotExists();

            return table;
        }

        public void RegenerateSitemap(string versionId)
        {
            var root = new SitemapEntry() {
                Name = "Root"
            };

            _Sitemap = DocPage.BuildSitemap(PartitionKey, RowKey, versionId);
            _Sitemap.Save(PartitionKey, RowKey, versionId);
        }
    }
}