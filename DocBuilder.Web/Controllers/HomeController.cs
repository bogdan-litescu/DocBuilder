using DocBuilder.Web.Models;
using DocBuilder.Web.Parsing;
using DocBuilder.Web.Source.Google;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Mvc.Filters;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Drive.v2;
using Google.Apis.Logging;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace DocBuilder.Web.Controllers
{
    public class HomeController : Controller
    {
    
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeAzureMvcApp(this, new AppFlowMetadata()).
                AuthorizeAsync(cancellationToken);

            if (result.Credential != null) {
                var service = new DriveService(new BaseClientService.Initializer {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "Doc Builder"
                });

                var site = new Site("dnnsharp", "action-form");
                site.Save();

                var version = new SiteVersion("dnnsharp", "action-form", "v3");
                version.Save();

                // this is our Action Form Documentation Folder
                var sync = new GoogleFolder(service, "0B2PogyBjJo-IR3A0aXliUnlvVEU", "dnnsharp", "action-form", "v3");
                await sync.Sync();

                site.RegenerateSitemap("v3");

                //CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount; // CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=<your_storage_name>;AccountKey=<your_account_key>");
                //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                //CloudTable table = tableClient.GetTableReference("raw");
                //table.DeleteIfExists();
                //table.CreateIfNotExists();

                //// Create the batch operation.
                //TableBatchOperation batchOperation = new TableBatchOperation();

                //foreach (var page in list.Items) {

                ////    var url = page.ExportLinks["text/html"];
                ////    //var htmlContents = new WebClient().DownloadString(url);
                ////    var htmlContents = await service.HttpClient.GetStringAsync(url);
                //////    HttpResponse resp = service.HttpClient.GetStringAsync(url);.RequestFactory().buildGetRequest(new GenericUrl(file.getDownloadUrl()))
                //////.execute();
                ////    //return resp.getContent();
                ////    var content = new HtmlParser().Parse(htmlContents);

                ////    content.PartitionKey = "DocSite1";
                ////    content.RowKey = page.Id;

                ////    batchOperation.Insert(content);

                //    ViewBag.Message = page.Id;
                //    // Execute the insert operation.

                //    //// Read storage
                //    //TableQuery<RawContent> query = new TableQuery<RawContent>()
                //    //      .Where(TableQuery.GenerateFilterCondition("PartitionKey",
                //    //          QueryComparisons.Equal, "Harp"));
                //    //var list = table.ExecuteQuery(query).ToList();
                //}

                ////table.ExecuteBatch(batchOperation);

                ////ViewBag.Message = "FILES COUNT IS: " + list.Items.Count();


                //TableOperation retrieveOperation = TableOperation.Retrieve<RawContent>("DocSite1", "1vkqHq9vgCI_jORaUcZTodwQCwGoGCUSXaYvUZS9XSHY");

                //// Execute the retrieve operation.
                //TableResult retrievedResult = table.Execute(retrieveOperation);

                //// Print the phone number of the result.
                //if (retrievedResult.Result != null)
                //    ViewBag.Content = ((RawContent)retrievedResult.Result).HtmContent;
                //else
                //    ViewBag.Content = "Invalid content";

                return View();
            } else {
                return new RedirectResult(result.RedirectUri);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }

    
}
