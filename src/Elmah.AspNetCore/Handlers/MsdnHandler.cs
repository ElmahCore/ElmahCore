using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Elmah.AspNetCore.Handlers;

internal static partial class Endpoints
{
    private static readonly MemoryCache Cache = new(Options.Create(new MemoryCacheOptions()));

    public static IEndpointConventionBuilder MapMsdn(this IEndpointRouteBuilder builder, string prefix = "")
    {
        return builder.MapMethods($"{prefix}/exception/{{*path}}", new[] { HttpMethods.Get, HttpMethods.Post }, async ([FromRoute] string path) =>
        {
            string json = (await Cache.GetOrCreateAsync(path, LoadAndCacheMsdnEntryAsync))!;
            return Results.Content(json, MediaTypeNames.Application.Json);
        });
    }

    public static IEndpointConventionBuilder MapMsdnStatus(this IEndpointRouteBuilder builder, string prefix = "")
    {
        return builder.MapMethods($"{prefix}/status/{{status}}", new[] { HttpMethods.Get, HttpMethods.Post }, async ([FromRoute] int status) =>
        {
            string json = (await Cache.GetOrCreateAsync($"status-{status}", LoadAndCacheStatusAsync))!;
            return Results.Content(json, MediaTypeNames.Application.Json);
        });
    }

    private static async Task<string> LoadAndCacheMsdnEntryAsync(ICacheEntry entry)
    {
        var url = "https://docs.microsoft.com/en-us/dotnet/api/" + entry.Key;
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(url);
        var nodes = doc.DocumentNode.SelectNodes("//div[@class='summaryHolder']/div[@class='summary clearFix']");
        if (nodes == null)
        {
            return "{}";
        }

        var links = doc.DocumentNode.SelectNodes("//div[@class='summaryHolder']/div[@class='summary clearFix']//a");

        if (links != null)
        {
            foreach (var link in links)
            {
                var href = link.Attributes["href"].Value;
                if (href == null || href.StartsWith("http"))
                {
                    continue;
                }

                link.SetAttributeValue("href",
                    href.StartsWith("/")
                        ? $"https://docs.microsoft.com{href}"
                        : $"https://docs.microsoft.com/en-us/dotnet/api/{href}");

                if (!link.Attributes.Contains("target"))
                {
                    link.SetAttributeValue("target", "_blank");
                }
            }
        }

        var html = nodes.FirstOrDefault()?.InnerHtml;

        if (string.IsNullOrEmpty(html))
        {
            return "{}";
        }

        return JsonSerializer.Serialize(new MsdnInfo
        {
            Path = url,
            Html = html
        }, DefaultJsonSerializerOptions.ApiSerializerOptions);
    }

    private static async Task<string> LoadAndCacheStatusAsync(ICacheEntry entry)
    {
        var url = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/" + ((string)entry.Key)[6..];
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(url);
        var nodes = doc.DocumentNode.SelectNodes("//article[@class='article']/div");
        if (nodes == null)
        {
            return "{}";
        }

        var links = doc.DocumentNode.SelectNodes("//article[@class='article']/div//a");

        if (links != null)
        {
            foreach (var link in links)
            {
                if (!link.Attributes.Contains("target"))
                {
                    link.SetAttributeValue("target", "_blank");
                }

                var href = link.Attributes["href"].Value;
                if (href == null || href.StartsWith("http"))
                {
                    continue;
                }

                link.SetAttributeValue("href",
                    href.StartsWith("/")
                        ? $"https://developer.mozilla.org{href}"
                        : $"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{href}");
            }
        }

        var html = nodes.FirstOrDefault()?.InnerHtml;

        if (string.IsNullOrEmpty(html))
        {
            return "{}";
        }

        return JsonSerializer.Serialize(new MsdnInfo
        {
            Path = url,
            Html = html
        }, DefaultJsonSerializerOptions.ApiSerializerOptions);
    }

    private class MsdnInfo
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Html { get; set; } = string.Empty;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Path { get; set; } = string.Empty;
    }
}