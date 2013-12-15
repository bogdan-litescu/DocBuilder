using DocBuilder.Web.Models;
using DocBuilder.Web.Parsing;
using DocBuilder.Web.Util;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DocBuilder.Web.Source.Google
{
    public class GoogleFolder
    {
        public string OwnerId { get; private set; }

        public string SiteId { get; private set; }

        public string VersionId { get; private set; }

        public string FolderId { get; private set; }

        public DriveService Service { get; private set; }

        public GoogleFolder(DriveService service, string folderId, string ownerId, string siteId, string versionId)
        {
            Service = service;
            FolderId = folderId;
            OwnerId = ownerId;
            SiteId = siteId;
            VersionId = versionId;
        }

        public async Task Sync()
        {
            await SyncFolder(FolderId, "");
        }

        async Task SyncFolder(string folderId, string localPathRelative)
        {
            // iterate all files and fodlers
            var req = Service.Files.List();
            //req.Q = "mimeType = 'application/vnd.google-apps.folder'";
            //req.Q = "mimeType = 'application/vnd.google-apps.document'";
            req.Q = string.Format("'{0}' in parents", folderId);
            var list = req.Execute();

            foreach (var item in list.Items) {
                if (item.MimeType == "application/vnd.google-apps.folder") {
                    // recursively iterate folders
                    var name = UriUtil.NameToUrlPart(item.Title);
                    await SyncFolder(item.Id, localPathRelative + name + "/");
                } else if (item.MimeType == "application/vnd.google-apps.document") {
                    // this is a document, queue it for processing
                    await ProcessDocument(item, localPathRelative);
                }
            }

        }

        async Task ProcessDocument(global::Google.Apis.Drive.v2.Data.File doc, string localPathRelative)
        {
            // TODO: for now, we put directly into DocPages, but we'll need to add some queueing and processing later

            var name = UriUtil.NameToUrlPart(doc.Title);
            var url = doc.ExportLinks["text/html"];
            var htmlContents = await Service.HttpClient.GetStringAsync(url);

            var docPage = new DocPage();
            docPage.OwnerId = OwnerId;
            docPage.SiteId = SiteId;
            docPage.VersionId = VersionId;
            docPage.PagePath = localPathRelative + name;
            docPage.Html = new HtmlParser().Parse(htmlContents).HtmContent; // +" <br /><br/>" + doc.Id;
            docPage.Save();
        }
    }
}