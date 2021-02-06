using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;

namespace ElmahCore.Mvc.Handlers
{
    internal static class MsdnHandler
    {
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

        public static async Task ProcessRequestException(HttpContext context, string path)
        {
            context.Response.ContentType = "application/json";
            string json = null;
            lock (Cache)
            {
                if (Cache.ContainsKey(path)) json = Cache[path];
            }

            if (json != null)
            {
                await context.Response.WriteAsync(json);
                return;
            }

            var url = "https://docs.microsoft.com/en-us/dotnet/api/" + path;
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var nodes = doc.DocumentNode.SelectNodes("//div[@class='summaryHolder']/div[@class='summary clearFix']");
            if (nodes == null)
            {
                await context.Response.WriteAsync("{}");
                return;
            }

            var links = doc.DocumentNode.SelectNodes("//div[@class='summaryHolder']/div[@class='summary clearFix']//a");

            if (links != null)
                foreach (var link in links)
                {
                    var href = link.Attributes["href"].Value;
                    if (href == null || href.StartsWith("http")) continue;

                    link.SetAttributeValue("href",
                        href.StartsWith("/")
                            ? $"https://docs.microsoft.com{href}"
                            : $"https://docs.microsoft.com/en-us/dotnet/api/{href}");

                    if (!link.Attributes.Contains("target"))
                        link.SetAttributeValue("target", "_blank");
                }

            var html = nodes.FirstOrDefault()?.InnerHtml;

            if (string.IsNullOrEmpty(html))
            {
                await context.Response.WriteAsync("{}");
                return;
            }

            json = JsonSerializer.Serialize(new MsdnInfo
            {
                Path = url,
                Html = html
            }, new JsonSerializerOptions
            {
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                MaxDepth = 0
            });
            lock (Cache)
            {
                if (!Cache.ContainsKey(path)) Cache.Add(path, json);
            }

            await context.Response.WriteAsync(json);
        }

        public static async Task ProcessRequestStatus(HttpContext context, string statusStr)
        {
            context.Response.ContentType = "application/json";
            if (!int.TryParse(statusStr, out var status))
            {
                await context.Response.WriteAsync("{}");
                return;
            }

            string json = null;
            lock (Cache)
            {
                if (Cache.ContainsKey("status-" + statusStr)) json = Cache["status-" + statusStr];
            }

            if (json != null)
            {
                await context.Response.WriteAsync(json);
                return;
            }

            var url = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/" + status;
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var nodes = doc.DocumentNode.SelectNodes("//article[@class='article']/div");
            if (nodes == null)
            {
                await context.Response.WriteAsync("{}");
                return;
            }

            var links = doc.DocumentNode.SelectNodes("//article[@class='article']/div//a");

            if (links != null)
                foreach (var link in links)
                {
                    if (!link.Attributes.Contains("target"))
                        link.SetAttributeValue("target", "_blank");

                    var href = link.Attributes["href"].Value;
                    if (href == null || href.StartsWith("http")) continue;

                    link.SetAttributeValue("href",
                        href.StartsWith("/")
                            ? $"https://developer.mozilla.org{href}"
                            : $"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{href}");
                }

            var html = nodes.FirstOrDefault()?.InnerHtml;

            if (string.IsNullOrEmpty(html))
            {
                await context.Response.WriteAsync("{}");
                return;
            }

            json = JsonSerializer.Serialize(new MsdnInfo
            {
                Path = url,
                Html = html
            }, new JsonSerializerOptions
            {
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                MaxDepth = 0
            });
            lock (Cache)
            {
                if (!Cache.ContainsKey("status-" + statusStr)) Cache.Add("status-" + statusStr, json);
            }

            await context.Response.WriteAsync(json);
        }

        private class MsdnInfo
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Html { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Path { get; set; }
        }
    }
}