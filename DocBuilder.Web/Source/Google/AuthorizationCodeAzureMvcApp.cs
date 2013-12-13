using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DocBuilder.Web.Source.Google
{
    public class AuthorizationCodeAzureMvcApp : AuthorizationCodeWebApp
    {
        // TODO(peleyal): we should also follow the MVC framework Authorize attribute

        private readonly Controller controller;
        private readonly FlowMetadata flowData;

        /// <summary>Gets the controller which is the owner of this authorization code MVC app instance.</summary>
        public Controller Controller { get { return controller; } }

        /// <summary>Gets the <seealso cref="Google.Apis.Auth.OAuth2.Mvc.FlowMetadata"/> object.</summary>
        public FlowMetadata FlowData { get { return flowData; } }

        /// <summary>Constructs a new authorization code MVC app using the given controller and flow data.</summary>
        public AuthorizationCodeAzureMvcApp(Controller controller, FlowMetadata flowData)
            : base(
            flowData.Flow,
            App.GetCurrentUrl(controller.Request, flowData.AuthCallback).ToString(),
            App.GetCurrentUrl(controller.Request).ToString())
        {
            this.controller = controller;
            this.flowData = flowData;
        }

        /// <summary>
        /// Asynchronously authorizes the installed application to access user's protected data. It gets the user 
        /// identifier by calling to <seealso cref="FlowData.GetUserId"/> and then calls to
        /// <seealso cref="AuthorizationCodeWebApp.AuthorizeAsync"/>.
        /// </summary>
        /// <param name="taskCancellationToken">Cancellation token to cancel an operation</param>
        /// <returns>
        /// Auth result object which contains the user's credential or redirect URI for the authorization server
        /// </returns>
        public Task<AuthResult> AuthorizeAsync(CancellationToken taskCancellationToken)
        {
            return base.AuthorizeAsync(FlowData.GetUserId(Controller), taskCancellationToken);
        }
    }

}
