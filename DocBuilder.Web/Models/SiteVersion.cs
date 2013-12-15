using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocBuilder.Web.Models
{
    public class SiteVersion : TableEntity
    {
        //public string Url { get; set; }

        //public string HtmContent { get; set; }

        //public PageMetadata Metadata { get; set; }

        public SiteVersion(string ownerId, string siteId, string versionId)
        {
            PartitionKey = ownerId + "." + siteId;
            RowKey = versionId;
        }

        public SiteVersion() { }

        public void Save()
        {
            var table = GetTable();

            var op = TableOperation.InsertOrReplace(this);
            table.Execute(op);
        }

        public static SiteVersion Get(string ownerId, string siteId, string versionId)
        {
            var table = GetTable();
            var op = TableOperation.Retrieve<SiteVersion>(ownerId, siteId);
            var retrievedResult = table.Execute(op);

            return (SiteVersion)retrievedResult.Result;
        }

        public static IEnumerable<SiteVersion> Get(string ownerId, string siteId)
        {
            var table = GetTable();

            var query = new TableQuery<SiteVersion>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ownerId + "." + siteId));

            return table.ExecuteQuery(query);
        }


        public static CloudTable GetTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount; // CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=<your_storage_name>;AccountKey=<your_account_key>");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("SiteVersions");
            table.CreateIfNotExists();

            return table;
        }
    }
}