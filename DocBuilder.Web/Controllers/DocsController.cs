using DocBuilder.Web.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DocBuilder.Web.Controllers
{
    public class DocsController : Controller
    {
        //
        // GET: /Docs/

        public ActionResult ShowPage(string ownerId, string siteId, string versionId, string pagePath)
        {
            var site = Site.Get(ownerId, siteId);
            if (site == null)
                throw new HttpException(404, "Not Found");

            if (string.IsNullOrEmpty(versionId ))
                versionId = site.CurrentVersionId;

            var page = DocPage.Get(ownerId, siteId, versionId, pagePath);
            if (page == null)
                throw new HttpException(404, "Not Found");

            ViewBag.PageContent = page.Html;
            ViewBag.Sitemap = site.GetSitemap(versionId);

            // print blob folders
            var container = DocPage.GetContainer();
            foreach (var item in container.ListBlobs()) {
                //if (item.GetType() == typeof(CloudBlobDirectory)) {
                //    var dir = item as CloudBlobDirectory;
                //    ViewBag.PageContent += "</br>" + dir.Prefix;
                //}
                ViewBag.PageContent += "</br>" + item.Uri.ToString();

            }

            return View();
        }

    }
}
