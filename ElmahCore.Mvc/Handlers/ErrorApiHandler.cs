using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ElmahCore.Mvc.Notifiers;
using Microsoft.AspNetCore.Http;

namespace ElmahCore.Mvc.Handlers
{
    internal static class ErrorApiHandler
    {
        public static async Task ProcessRequest(HttpContext context, ErrorLog errorLog, string path)
        {
            context.Response.ContentType = "application/json";
            List<ErrorLogFilter> filters;
            string searchText;
            
            switch (path)
            {
                case "api/error":
                    var errorId = context.Request.Query["id"].ToString();
                    if (string.IsNullOrEmpty(errorId)) await context.Response.WriteAsync("{}");

                    var error = await GetErrorAsync(errorLog, errorId);

                    var jRes = JsonSerializer.Serialize(error, new JsonSerializerOptions
                    {
                        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        MaxDepth = 0
                    });
                    await context.Response.WriteAsync(jRes);
                    break;

                case "api/errors":
                    int.TryParse(context.Request.Query["i"].ToString(), out var errorIndex);
                    int.TryParse(context.Request.Query["s"].ToString(), out var pageSize);
                    searchText = context.Request.Query["q"].ToString();
                    filters = await ReadErrorFilters(context.Request);

                    var entities = await GetErrorsAsync(searchText, errorLog, filters, errorIndex, pageSize);

                    var json = JsonSerializer.Serialize(entities, new JsonSerializerOptions
                    {
                        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        MaxDepth = 0
                    });
                    await context.Response.WriteAsync(json);
                    break;

                case "api/new-errors":
                    var id = context.Request.Query["id"].ToString();
                    searchText = context.Request.Query["q"].ToString();
                    filters = await ReadErrorFilters(context.Request);

                    var newEntities = await GetNewErrorsAsync(searchText, errorLog, id, filters);

                    var jsonResult = JsonSerializer.Serialize(newEntities, new JsonSerializerOptions
                    {
                        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        MaxDepth = 0
                    });
                    await context.Response.WriteAsync(jsonResult);
                    break;
            }
        }

        private static async Task<List<ErrorLogFilter>> ReadErrorFilters(HttpRequest request)
        {
            if (request.Method != "POST") return new List<ErrorLogFilter>();
            
            var filters = new List<ErrorLogFilter>();
            var strings = await JsonSerializer.DeserializeAsync<List<string>>(request.Body);
            foreach (var str in strings)
            {
                var filter = ErrorLogFilter.Parse(str);
                if (filter != null) filters.Add(filter);
            }

            return filters;

        }

        private static async Task<ErrorLogEntryWrapper> GetErrorAsync(ErrorLog errorLog, string id)
        {
            var error = await errorLog.GetErrorAsync(id);
            return error == null ? null : new ErrorLogEntryWrapper(error);
        }

        private static async Task<ErrorsList> GetErrorsAsync(string searchText, ErrorLog errorLog, List<ErrorLogFilter> errorFilters,
            int errorIndex, int pageSize)
        {
            if (errorIndex < 0) errorIndex = 0;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var entries = new List<ErrorLogEntry>(pageSize);
            var totalCount = await errorLog.GetErrorsAsync(searchText, errorFilters, errorIndex, pageSize, entries);
            return new ErrorsList
            {
                Errors = entries.Select(i => new ErrorLogEntryWrapper(i)).ToList(),
                TotalCount = totalCount
            };
        }

        private static async Task<ErrorsList> GetNewErrorsAsync(string searchText, ErrorLog errorLog, string id,
            List<ErrorLogFilter> errorFilters)
        {
            if (string.IsNullOrEmpty(id)) return await GetErrorsAsync(searchText, errorLog, errorFilters, 0, 50);
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