using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elmah.AspNetCore.Handlers;

internal static partial class Endpoints
{
    public static IEndpointConventionBuilder MapApiError(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var handler = RequestDelegateFactory.Create(async (
            [FromQuery] string? id,
            [FromServices] ErrorLog errorLog,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid errorGuid))
            {
                return Results.Content("{}", MediaTypeNames.Application.Json);
            }

            var error = await GetErrorAsync(errorLog, errorGuid, cancellationToken);
            return Results.Json(error, DefaultJsonSerializerOptions.ApiSerializerOptions);
        });

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.MapMethods($"{prefix}/api/error", new[] { HttpMethods.Get, HttpMethods.Post }, pipeline.Build());
    }

    public static IEndpointConventionBuilder MapApiErrors(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var handler = RequestDelegateFactory.Create(async (
            [FromQuery(Name = "i")] int? errorIndex,
            [FromQuery(Name = "s")] int? pageSize,
            [FromQuery(Name = "q")] string? searchText,
            [FromServices] ErrorLog errorLog,
            HttpRequest request,
            CancellationToken cancellationToken) =>
        {
            var filters = await ReadErrorFilters(request);

            var entities = await GetErrorsAsync(searchText, errorLog, filters, errorIndex ?? 0, pageSize ?? 0, cancellationToken);
            return Results.Json(entities, DefaultJsonSerializerOptions.ApiSerializerOptions);
        });

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.MapMethods($"{prefix}/api/errors", new[] { HttpMethods.Get, HttpMethods.Post }, pipeline.Build());
    }

    public static IEndpointConventionBuilder MapApiNewErrors(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var handler = RequestDelegateFactory.Create(async (
            [FromQuery] string? id,
            [FromQuery(Name = "q")] string? searchText,
            [FromServices] ErrorLog errorLog,
            HttpRequest request,
            CancellationToken cancellationToken) =>
        {
            var filters = await ReadErrorFilters(request);

            var newEntities = await GetNewErrorsAsync(searchText, errorLog, id, filters, cancellationToken);
            return Results.Json(newEntities, DefaultJsonSerializerOptions.ApiSerializerOptions);
        });

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.MapPost($"{prefix}/api/new-errors", pipeline.Build());
    }

    private static async Task<ErrorLogFilter[]> ReadErrorFilters(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method))
        {
            return Array.Empty<ErrorLogFilter>();
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

        return filters.ToArray();
    }

    private static async Task<ErrorLogEntryWrapper?> GetErrorAsync(ErrorLog errorLog, Guid id, CancellationToken cancellationToken)
    {
        var error = await errorLog.GetErrorAsync(id, cancellationToken);
        return error == null ? null : new ErrorLogEntryWrapper(error);
    }

    private static async Task<ErrorsList> GetErrorsAsync(string? searchText, ErrorLog errorLog, ErrorLogFilter[] errorFilters,
        int errorIndex, int pageSize, CancellationToken cancellationToken)
    {
        errorIndex = Math.Max(0, errorIndex);
        pageSize = pageSize switch
        {
            < 0 => 0,
            > 100 => 100,
            _ => pageSize
        };

        var entries = new List<ErrorLogEntry>(pageSize);
        var totalCount = await errorLog.GetErrorsAsync(searchText, errorFilters, errorIndex, pageSize, entries, cancellationToken);
        return new ErrorsList
        {
            Errors = entries.Select(i => new ErrorLogEntryWrapper(i)).ToList(),
            TotalCount = totalCount
        };
    }

    private static async Task<ErrorsList> GetNewErrorsAsync(string? searchText, ErrorLog errorLog, string? id,
        ErrorLogFilter[] errorFilters, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid errorGuid))
        {
            return await GetErrorsAsync(searchText, errorLog, errorFilters, 0, 50, cancellationToken);
        }

        var entries = new List<ErrorLogEntry>();
        var totalCount = await errorLog.GetNewErrorsAsync(searchText, errorFilters, errorGuid, entries, cancellationToken);
        return new ErrorsList
        {
            Errors = entries.Select(i => new ErrorLogEntryWrapper(i)).ToList(),
            TotalCount = totalCount
        };
    }
}