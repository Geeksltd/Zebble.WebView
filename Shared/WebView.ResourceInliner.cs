namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HtmlAgilityPack;

    partial class WebView
    {
        class ResourceInliner
        {
            string Html;
            HtmlDocument Doc = new HtmlDocument();
            List<KeyValuePair<string, string>> ExistingInlineScriptReplacements = new List<KeyValuePair<string, string>>();
            Dictionary<string, string> CodeReplacements = new Dictionary<string, string>();
            HtmlNode Root => Doc.DocumentNode;
            public string ResourceNamespace;
            public Assembly ResourceAssembly;
            public bool MergeExternalResources;

            public ResourceInliner(string html) => Html = html;

            internal string Inline()
            {
                EscapeInlineScripts();

                try { Doc.LoadHtml(Html); }
                catch (Exception ex) { throw new Exception("Failed to load the specified html: " + ex); }

                foreach (var link in GetCssReferences())
                    InlineReferencedFile(link, "href", "style");

                foreach (var script in GetScriptReferences())
                    InlineReferencedFile(script, "src", "script");

                foreach (var img in Root.Descendants("img"))
                    InlineReferencedFile(img, "src", "img");

                Html = Root.InnerHtml;

                foreach (var r in CodeReplacements.Where(x => Html.Contains(x.Key)).ToArray())
                {
                    Html = Html.Replace(r.Key, r.Value);
                    CodeReplacements.Remove(r.Key);
                }

                UnEscapeInlineScripts();

                return Html;
            }

            IEnumerable<HtmlNode> GetCssReferences()
            {
                return Root.Descendants("link").Where(x => x.GetAttributeValue("rel", "") == "stylesheet");
            }

            IEnumerable<HtmlNode> GetScriptReferences()
            {
                return Root.Descendants("script").Where(x => x.GetAttributeValue("src", "").HasValue());
            }

            void EscapeInlineScripts()
            {
                foreach (var starter in new[] { "<script>", "<script type=\"text/javascript\">", "<script type='text/javascript'>" })
                {
                    var start = 0;
                    while (true)
                    {
                        start = Html.IndexOf(starter, start);
                        if (start == -1) break;
                        else start += starter.Length;

                        var end = Html.IndexOf("</script>", start);
                        if (end == -1) break;

                        var script = Html.Substring(start, end - start);
                        var key = Guid.NewGuid().ToString();
                        ExistingInlineScriptReplacements.Add(key, script);
                        Html = Html.Replace(script, key);
                        start = Html.IndexOf("</script>", start);
                    }
                }
            }

            void UnEscapeInlineScripts()
            {
                foreach (var item in ExistingInlineScriptReplacements)
                    Html = Html.Replace(item.Key, item.Value);
            }

            void InlineReferencedFile(HtmlNode node, string attribute, string tag)
            {
                var url = node.GetAttributeValue(attribute, "");
                var data = ReadResourceBytes(url);
                if (data.None()) return; // Not to be replaced.

                node.Name = tag;
                if (tag == "script")
                {
                    node.Attributes.RemoveWhere(x => x.Name.IsAnyOf("type", "src"));
                    node.Attributes.Add("type", "text/javascript");
                }

                if (tag == "img")
                {
                    var fileType = "png";
                    if (url.ToLowerOrEmpty().EndsWithAny("jpg", "jpeg")) fileType = "jpeg";
                    if (url.ToLowerOrEmpty().EndsWith("gif")) fileType = "gif";

                    node.Attributes.Add("src", "data:image/" + fileType + ";base64," + data.ToBase64String().HtmlEncode());
                }
                else
                {
                    var code = Guid.NewGuid().ToString();
                    CodeReplacements.Add(code, data.ToString(System.Text.Encoding.UTF8).WithWrappers("\r\n", "\r\n"));
                    node.AppendChild(HtmlNode.CreateNode(code));
                }
            }

            byte[] ReadResourceBytes(string url)
            {
                if (url.LacksValue()) return new byte[0];

                if (url.StartsWith("resource:"))
                {
                    if (ResourceAssembly == null)
                        throw new Exception($"Failed to load '{url}' as no resourceAssembly is specified for this WebView.");

                    if (ResourceNamespace.LacksValue())
                        throw new Exception($"Failed to load '{url}' as no resource namespace is specified for this WebView.");

                    return ResourceAssembly.ReadEmbeddedResource(ResourceNamespace, url.TrimStart("resource:"));
                }

                if (url.Contains(":")) return new byte[0]; // External URL

                if (!MergeExternalResources) return new byte[0];

                var file = Device.IO.File(url);
                if (!file.Exists())
                    throw new Exception("Web resource file not found: '" + url + "'.");

                return file.ReadAllBytes();
            }
        }
    }
}