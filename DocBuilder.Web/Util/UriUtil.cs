using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DocBuilder.Web.Util
{
    public static class UriUtil
    {
        public static string NameToUrlPart(string name)
        {
            return Regex.Replace(name, "[^a-z]+", "-", RegexOptions.IgnoreCase).Trim(' ', '-').ToLower();
        }

        public static bool IsRelativeToCurrentFolder(string url)
        {
            return url[0] != '/' && url.IndexOf("http://") != 0 && url.IndexOf("https://") != 0;
        }

        [Obsolete("Dont's use HttpRuntime.paths directly, see the App class")]
        public static string ToAbsoulteUri(string baseUri, string relUri)
        {
            if (HttpRuntime.AppDomainAppVirtualPath != "/")
                baseUri = Regex.Replace(baseUri, HttpRuntime.AppDomainAppVirtualPath, "", RegexOptions.IgnoreCase);
            relUri = relUri.TrimEnd('&');

            if (relUri.IndexOf("http") == 0)
                return relUri;

            if (baseUri.IndexOf("http") != 0)
                baseUri = "http://" + baseUri;

            if (relUri.StartsWith("~/"))
                relUri = HttpRuntime.AppDomainAppVirtualPath.TrimEnd('/') + "/" + relUri.Substring(2); // VirtualPathUtility.ToAbsolute(relUri);

            Uri uri = new Uri(baseUri);
            return "http://" + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port.ToString()) + "/" + relUri.Trim('/');
        }

        public static string ToAbsoluteUrl(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return relativeUrl;

            if (relativeUrl.IndexOf("http") == 0)
                return relativeUrl;

            if (HttpContext.Current == null)
                return relativeUrl;

            if (relativeUrl.StartsWith("/"))
                relativeUrl = relativeUrl.Insert(0, "~");
            if (!relativeUrl.StartsWith("~/"))
                relativeUrl = relativeUrl.Insert(0, "~/");

            Uri url = HttpContext.Current.Request.Url;
            string port = url.Port != 80 ? (":" + url.Port) : string.Empty;
            string query = relativeUrl.IndexOf("?") != -1 ? relativeUrl.Substring(relativeUrl.IndexOf("?")) : "";
            relativeUrl = relativeUrl.IndexOf("?") != -1 ? relativeUrl.Substring(0, relativeUrl.IndexOf("?")) : relativeUrl;

            return string.Format("{0}://{1}{2}{3}{4}", url.Scheme, url.Host, port, VirtualPathUtility.ToAbsolute(relativeUrl), query);
        }

        public static string ToRelativeUrl(string absoluteUrl)
        {
            if (absoluteUrl.IndexOf("http") != 0)
                return absoluteUrl;

            // return after the first slash, but skip the https:// slashes
            return absoluteUrl.Substring(absoluteUrl.IndexOf('/', 10));
        }

        public static string DropExtension(string url)
        {
            var ext = VirtualPathUtility.GetExtension(url);
            if (!string.IsNullOrEmpty(ext))
                url = url.Substring(0, url.LastIndexOf(ext));
            return url;
        }

        public static string Sanitize(string url, bool allowAbsolute)
        {
            if (string.IsNullOrEmpty(url))
                return "/";

            if (url.IndexOf("http://", StringComparison.OrdinalIgnoreCase) == 0 || url.IndexOf("https://", StringComparison.OrdinalIgnoreCase) == 0) {
                
                // absolute URLs only allowed for redirects - we can't rewrite to a different address
                if (allowAbsolute) 
                    return url;
                
                var iOfRootSlash = url.IndexOf('/', 8);
                return iOfRootSlash == -1 ? "/" : url.Substring(iOfRootSlash);
            }

            // should begin with a slash
            // if (url[0] != '/')
            //     return '/' + url;

            return url;
        }

        public static bool IsIp(string host)
        {
            // TODO: match anywhere or not?
            return Regex.IsMatch(host, ".*\\d+\\.\\d+\\.\\d+\\.\\d+.*");
        }

        public static string BuildQueryString(NameValueCollection queryParams, bool prependQuestionMark = false, 
            bool lowercaseNames = false, Func<string, string> tokenize = null, string[] skipParams = null)
        {
            if (queryParams == null || queryParams.Count == 0)
                return "";

            var sb = new StringBuilder();
            if (prependQuestionMark)
                sb.Append('?');

            foreach (var key in queryParams.AllKeys) {

                if (key == null)
                    continue;

                var name = tokenize == null ? key : tokenize(key);
                if (skipParams != null && skipParams.Contains(name))
                    continue;

                if (lowercaseNames)
                    name = name.ToLower();
                name = HttpUtility.UrlEncode(name);

                var value = tokenize == null ? queryParams[key] : tokenize(queryParams[key]);
                value = HttpUtility.UrlEncode(value)
                    .Replace("%24", "$"); // don't escape $

                sb.AppendFormat("{0}={1}&", name, value);
            }

            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static string ResolvePortalUrl(string appPath, string portalRelativeUrl)
        {
            portalRelativeUrl = portalRelativeUrl.Trim('~');
            return (appPath.TrimEnd('/') + "/" + portalRelativeUrl.Trim('/')).TrimEnd('/');
        }

        public static string GetPath(string url)
        {
            if (url.IndexOf('?') > 0)
                return url.Substring(0, url.IndexOf('?'));
            return url;
        }

        public static string GetQuery(string url)
        {
            if (url.IndexOf('?') > 0)
                return url.Substring(url.IndexOf('?') + 1);
            return "";
        }

        /// <summary>
        /// Removes parameters from query string
        /// </summary>
        /// <param name="fromUrl"></param>
        /// <param name="skipParams"></param>
        /// <returns></returns>
        public static string CleanQuery(string fromUrl, params string[] skipParams)
        {
            return BuildQueryString(ParseQuery(fromUrl, skipParams), true);
        }


        /// <summary>
        /// normalizes query, since sometimes DNN comes with ampersands instead of equals, like ~/Default.aspx?tabId=123&product&NAVXP
        /// </summary>
        /// <param name="fromPath"></param>
        /// <returns></returns>
        public static NameValueCollection ParseQuery(string fromPath, params string[] skipParams)
        {
            var query = fromPath.Substring(fromPath.IndexOf('?') + 1)
                .TrimStart('/'); // sometimes the path looks like this /Command=Core_Download&EntryId=750

            // fix duplicate &&, for example /page.aspx?q=1&&p=2
            query = query.Replace("&&", "&");

            var expectedNext = '=';
            var sbParse = new StringBuilder();
            for (var i = 0; i < query.Length; i++) {
                var c = query[i];

                // check if we need to rewrite & to =
                if (c == '&') {
                    if (expectedNext == '=') {
                        c = '=';
                    } else {
                        // we don't need to rewrite it. next time we expect a =
                        expectedNext = '=';
                    }
                }

                // if this is a =, next time we expect a &
                if (c == '=')
                    expectedNext = '&';

                sbParse.Append(c);
            }

            var queryStringCollection = HttpUtility.ParseQueryString(sbParse.ToString());
            if (skipParams != null) {
                foreach (var p in skipParams)
                    queryStringCollection.Remove(p);
            }

            return queryStringCollection;
        }
    }
}
