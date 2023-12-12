using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ElmahCore.Mvc.Notifiers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ElmahCore.Mvc.Handlers
{
    internal static partial class Endpoints
    {
        public static IEndpointConventionBuilder MapApiError(this IEndpointRouteBuilder builder, string prefix = "")
        {
            return builder.MapMethods($"{prefix}/api/error", new[] { HttpMethods.Get, HttpMethods.Post }, async ([FromQuery] string? id, [FromServices] ErrorLog errorLog) =>
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Results.Content("{}", "application/json");
                }

                var error = await GetErrorAsync(errorLog, id);
                return Results.Json(error, DefaultJsonSerializerOptions.ApiSerializerOptions);
            });
        }

        public static IEndpointConventionBuilder MapApiErrors(this IEndpointRouteBuilder builder, string prefix = "")
        {
            return builder.MapMethods($"{prefix}/api/errors", new[] { HttpMethods.Get, HttpMethods.Post }, async (
                [FromQuery(Name = "i")] int? errorIndex,
                [FromQuery(Name = "s")] int? pageSize,
                [FromQuery(Name = "q")] string? searchText,
                [FromServices] ErrorLog errorLog,
                HttpRequest request) =>
            {
                var filters = await ReadErrorFilters(request);

                var entities = await GetErrorsAsync(searchText, errorLog, filters, errorIndex ?? 0, pageSize ?? 0);
                return Results.Json(entities, DefaultJsonSerializerOptions.ApiSerializerOptions);
            });
        }

        public static IEndpointConventionBuilder MapApiNewErrors(this IEndpointRouteBuilder builder, string prefix = "")
        {
            return builder.MapMethods($"{prefix}/api/new-errors", new[] { HttpMethods.Post, HttpMethods.Get }, async (
                [FromQuery] string id,
                [FromQuery(Name = "q")] string? searchText,
                [FromServices] ErrorLog errorLog,
                HttpRequest request) =>
            {
                var filters = await ReadErrorFilters(request);

                var newEntities = await GetNewErrorsAsync(searchText, errorLog, id, filters);
                return Results.Json(newEntities, DefaultJsonSerializerOptions.ApiSerializerOptions);
            });
        }

        private static async Task<List<ErrorLogFilter>> ReadErrorFilters(HttpRequest request)
        {
            if (!HttpMethods.IsPost(request.Method))
            {
                return new List<ErrorLogFilter>();
            }

            var filters = new List<ErrorLogFilter>();
            var strings = await JsonSerializer.DeserializeAsync<string[]>(request.Body) ?? Array.Empty<string>();
            foreach (var str in strings)
            {
                var filter = ErrorLogFilter.Parse(str);
                if (filter != null)
                {
                    filters.Add(filter);
                }
            }

            return filters;
        }

        private static async Task<ErrorLogEntryWrapper?> GetErrorAsync(ErrorLog errorLog, string id)
        {
            var error = await errorLog.GetErrorAsync(id);
            return error == null ? null : new ErrorLogEntryWrapper(error);
        }

        private static async Task<ErrorsList> GetErrorsAsync(string? searchText, ErrorLog errorLog, List<ErrorLogFilter> errorFilters,
            int errorIndex, int pageSize)
        {
            errorIndex = Math.Max(0, errorIndex);
            pageSize = pageSize switch
            {
                < 0 => 0,
                > 100 => 100,
                _ => pageSize
            };

            var entries = new List<ErrorLogEntry>(pageSize);
            var totalCount = await errorLog.GetErrorsAsync(searchText, errorFilters, errorIndex, pageSize, entries);
            return new ErrorsList
            {
                Errors = entries.Select(i => new ErrorLogEntryWrapper(i)).ToList(),
                TotalCount = totalCount
            };
        }

        private static async Task<ErrorsList> GetNewErrorsAsync(string? searchText, ErrorLog errorLog, string id,
            List<ErrorLogFilter> errorFilters)
        {
            if (string.IsNullOrEmpty(id))
            {
                return await GetErrorsAsync(searchText, errorLog, errorFilters, 0, 50);
            }

            var entries = new List<ErrorLogEntry>();
            var totalCount = await errorLog.GetNewErrorsAsync(searchText, errorFilters, id, entries);
            return new ErrorsList
            {
                Errors = entries.Select(i => new ErrorLogEntryWrapper(i)).ToList(),
                TotalCount = totalCount
            };
        }
    }
}