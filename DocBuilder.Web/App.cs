using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocBuilder.Web
{
    public class App
    {
        #region Singleton
        
        static App _Instance;

        static App()
        {
            _Instance = new App();
        }
        
        public static App Instance { get { return _Instance; } }

        #endregion


        private App()
        {
        }

        #region Environment

        /// <summary>
        /// On Azure, the public URL needs to be computed from host header
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Uri GetCurrentUrl(HttpRequestBase request, string setRelativeUrl = null)
        {
            string host = request.Headers["host"] != null 
                ? request.Headers["host"]
                : request.Url.GetLeftPart(UriPartial.Authority);

            setRelativeUrl = setRelativeUrl ?? request.Url.PathAndQuery;

            return new Uri(request.Url.Scheme + "://" + host + "/" + setRelativeUrl.TrimStart('/'));
        }

        #endregion


    }
}