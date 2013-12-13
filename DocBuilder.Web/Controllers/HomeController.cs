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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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
                    ApplicationName = "ASP.NET MVC Sample"
                });

                // YOUR CODE SHOULD BE HERE..
                // SAMPLE CODE:
                var req = service.Files.List();
                //req.Q = "mimeType = 'application/vnd.google-apps.folder'";
                //req.Q = "mimeType = 'application/vnd.google-apps.document'";
                req.Q = "title = 'Home'";
                var list = await req.ExecuteAsync();
                ViewBag.Message = "FILES COUNT IS: " + list.Items.Count();
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
