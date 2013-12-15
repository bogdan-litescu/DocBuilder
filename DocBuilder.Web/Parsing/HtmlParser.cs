using DocBuilder.Web.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DocBuilder.Web.Parsing
{
    public class HtmlParser
    {
        public RawContent Parse(string contents)
        {
            var raw = new RawContent();

            // convert to XHTML
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(contents);

            // this is plain page, extract and index as HTML
            //raw.Metadata = ReadMeta(doc);

            // extract body
            HtmlNode bodyNode = ExtractBody(doc);
            raw.HtmContent = bodyNode.InnerHtml;

            return raw;
        }

        PageMetadata ReadMeta(HtmlDocument doc)
        {
            var meta = new PageMetadata();

            // read title
            var metaTilte = doc.DocumentNode.SelectSingleNode("//head/title");
            if (metaTilte != null)
                meta.Title = metaTilte.InnerText.Trim();

            // read meta tags
            var allMetaTags = doc.DocumentNode.SelectNodes("//head/meta");
            foreach (var tag in allMetaTags) {
                
                if (!tag.Attributes.Contains("name"))
                    continue;

                var value = "";
                if (tag.Attributes.Contains("content"))
                    value = tag.Attributes["content"].Value.Trim();

                switch (tag.Attributes["name"].Value.ToLower()) {
                    case "description":
                        meta.Description = value;
                        break;
                    case "keywords":
                        meta.Keywords = value;
                        break;
                }
            }

            return meta;
        }


        HtmlNode ExtractBody(HtmlDocument doc)
        {
            HtmlNode bodyNode = null;
            if (doc.DocumentNode.SelectSingleNode("//body") != null) {
                bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            } else {
                bodyNode = doc.DocumentNode;
            }

            // remove all object and script nodes from body
            foreach (var attr in new string[] { "script", "object" }) {
                var nodes = bodyNode.SelectNodes("//" + attr);
                if (nodes != null) {
                    foreach (var script in nodes) {
                        script.Remove();
                    }
                }
            }

            return bodyNode;
        }
    }
}