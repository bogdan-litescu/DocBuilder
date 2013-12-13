using Google;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Mvc.Controllers;
using Google.Apis.Auth.OAuth2.Mvc.Filters;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DocBuilder.Web.Source.Google
{
    public class GoogleAuthAzureCallbackController : AuthCallbackController
    {
        protected override FlowMetadata FlowData
        {
            get { return new AppFlowMetadata(); }
        }

        [AsyncTimeout(10000)]
        public async override Task<ActionResult> IndexAsync(AuthorizationCodeResponseUrl authorizationCode,
            CancellationToken taskCancellationToken)
        {
            if (string.IsNullOrEmpty(authorizationCode.Code)) {
                var errorResponse = new TokenErrorResponse(authorizationCode);
                Logger.Info("Received an error. The response is: {0}", errorResponse);

                return OnTokenError(errorResponse);
            }

            Logger.Debug("Received \"{0}\" code", authorizationCode.Code);

            var returnUrl = App.GetCurrentUrl(Request).ToString(); // Request.Url.ToString();
            returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?"));

            var token = await Flow.ExchangeCodeForTokenAsync(UserId, authorizationCode.Code, returnUrl,
                taskCancellationToken);

            // Extract the right state.
            var oauthState = await AuthWebUtility.ExtracRedirectFromState(Flow.DataStore, UserId, authorizationCode.State);

            return new RedirectResult(oauthState);
        }
    }
}