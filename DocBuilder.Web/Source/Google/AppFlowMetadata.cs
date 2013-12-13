using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Util.Store;
using System;
using System.Web.Mvc;

namespace DocBuilder.Web.Source.Google
{
    public class AppFlowMetadata : FlowMetadata
    {
        private static readonly string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };

        private static readonly IAuthorizationCodeFlow flow =
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer {
                ClientSecrets = new ClientSecrets {
                    ClientId = "409737316609.apps.googleusercontent.com",
                    ClientSecret = "NEWEeXaiqJUPS0POMKhucq7h"
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new FileDataStore("Drive.Api.Auth.Store")
            });

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should manage your users accounts. 
            // We consider providing an "OpenId Connect" library for helping you doing that.
            var user = controller.Session["user"];
            if (user == null) {
                user = Guid.NewGuid();
                controller.Session["user"] = user;
            }
            return user.ToString();

        }

        public override IAuthorizationCodeFlow Flow
        {
            get { return flow; }
        }

        public override string AuthCallback
        {
            get { return "/GoogleAuthAzureCallback/IndexAsync"; }
        }

    }
}