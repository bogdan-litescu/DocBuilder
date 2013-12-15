using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocBuilder.Web.Models
{
    public class RawContent : TableEntity
    {
        public string Url { get; set; }

        public string HtmContent { get; set; }

        //public PageMetadata Metadata { get; set; }

        public RawContent(string customerId, string contentId)
        {
            PartitionKey = customerId;
            RowKey = contentId;
        }

        public RawContent() { }
    }
}