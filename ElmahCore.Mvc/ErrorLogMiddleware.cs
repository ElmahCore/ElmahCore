using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElmahCore.Mvc.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElmahCore.Mvc
{
    internal sealed class ErrorLogMiddleware
    {
        internal static bool ShowDebugPage = false;

        private static readonly string[] SupportedContentTypes =
        {
            "application/json",
            "application/x-www-form-urlencoded",
            "application/javascript",
            "application/soap+xml",
            "application/xhtml+xml",
            "application/xml",
            "text/html",
            "text/javascript",
            "text/plain",
            "text/xml",
            "text/markdown"
        };

        private readonly Func<HttpContext, bool> _checkPermissionAction = context => true;
        private readonly string _elmahRoot = @"~/elmah";
        private readonly ErrorLog _errorLog;
        private readonly IElmahExceptionLogger _elmahLogger;
        private readonly ILogger _logger;
        private readonly bool _logRequestBody = true;
        private readonly RequestDelegate _next;
        private readonly Func<HttpContext, Error, Task> _onError = (context, error) => Task.CompletedTask;

        public ErrorLogMiddleware(
            RequestDelegate next,
            ErrorLog errorLog,
            ILoggerFactory loggerFactory,
            IElmahExceptionLogger elmahLogger,
            IOptions<ElmahOptions> elmahOptions)
        {
            _next = next;
            _errorLog = errorLog ?? throw new ArgumentNullException(nameof(errorLog));
            _elmahLogger = elmahLogger;
            var lf = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _logger = lf.CreateLogger<ErrorLogMiddleware>();

            //return here if the elmah options is not provided
            if (elmahOptions?.Value == null)
                return;
            var options = elmahOptions.Value;

            _checkPermissionAction = options.PermissionCheck;
            _onError = options.Error;

            _logRequestBody = elmahOptions.Value?.LogRequestBody == true;

            if (elmahOptions.Value != null)
            {
                if (!string.IsNullOrEmpty(options.Path))
                {
                    _elmahRoot = elmahOptions.Value.Path.ToLower();
                    if (!_elmahRoot.StartsWith("/") && !_elmahRoot.StartsWith("~/")) _elmahRoot = "/" + _elmahRoot;
                    if (_elmahRoot.EndsWith("/")) _elmahRoot = _elmahRoot.Substring(0, _elmahRoot.Length - 1);
                }

                if (!string.IsNullOrWhiteSpace(options.ApplicationName))
                    _errorLog.ApplicationName = elmahOptions.Value.ApplicationName;
                if (options.SourcePaths != null && options.SourcePaths.Any())
                    _errorLog.SourcePaths = elmahOptions.Value.SourcePaths;
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string body = null;
            var elmahRoot = _elmahRoot;
            try
            {
                if (elmahRoot.StartsWith("~/"))
                    elmahRoot = context.Request.PathBase + elmahRoot.Substring(1);

                context.Features.Set<IElmahLogFeature>(new ElmahLogFeature());

                var sourcePath = context.Request.PathBase + context.Request.Path.Value;
                if (sourcePath.Equals(elmahRoot, StringComparison.InvariantCultureIgnoreCase)
                    || sourcePath.StartsWith(elmahRoot + "/", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!_checkPermissionAction(context))
                    {
                        await context.ChallengeAsync();
                        return;
                    }

                    var path = sourcePath.Substring(elmahRoot.Length, sourcePath.Length - elmahRoot.Length);
                    if (path.StartsWith("/")) path = path.Substring(1);
                    if (path.Contains('?')) path = path.Substring(0, path.IndexOf('?'));
                    await ProcessElmahRequest(context, path);
                    return;
                }

                var ct = context.Request.ContentType?.ToLower();
                var tEnc = string.Join(",", context.Request.Headers["Transfer-Encoding"].ToArray());
                if (_logRequestBody && !string.IsNullOrEmpty(ct) && SupportedContentTypes.Any(i => ct.Contains(ct))
                    && !tEnc.Contains("chunked"))
                    body = await GetBody(context.Request);

                await _next(context);

                if (context.Response.HasStarted
                    || context.Response.StatusCode < 400
                    || context.Response.StatusCode >= 600
                    || context.Response.ContentLength.HasValue
                    || !string.IsNullOrEmpty(context.Response.ContentType))
                    return;
                await _elmahLogger.LogExceptionAsync(context, new HttpException(context.Response.StatusCode), _onError, body);
            }
            catch (Exception exception)
            {
                var entry = await _elmahLogger.LogExceptionAsync(context, exception, _onError, body);
                var location = $"{elmahRoot}/detail/{entry.Id}";

                context.Features.Set<IElmahFeature>(new ElmahFeature(entry.Id, location));

                //To next middleware
                if (!ShowDebugPage) throw;
                //Show Debug page
                context.Response.Redirect(location);
            }
        }

        private async Task<string> GetBody(HttpRequest request)
        {
            request.EnableBuffering();
            var body = request.Body;
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            // ReSharper disable once MustUseReturnValue
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            body.Seek(0, SeekOrigin.Begin);
            request.Body = body;

            return bodyAsText;
        }

        private async Task ProcessElmahRequest(HttpContext context, string resource)
        {
            try
            {
                var elmahRoot = (_elmahRoot.StartsWith("~/"))
                    ? context.Request.PathBase + _elmahRoot.Substring(1)
                    : _elmahRoot;

                if (resource.StartsWith("api/"))
                {
                    await ErrorApiHandler.ProcessRequest(context, _errorLog, resource);
                    return;
                }

                if (resource.StartsWith("exception/"))
                {
                    await MsdnHandler.ProcessRequestException(context, resource.Substring("exception/".Length));
                    return;
                }

                if (resource.StartsWith("status/"))
                {
                    await MsdnHandler.ProcessRequestStatus(context, resource.Substring("status/".Length));
                    return;
                }

                switch (resource)
                {
                    case "xml":
                        await ErrorXmlHandler.ProcessRequest(context, _errorLog);
                        break;
                    case "json":
                        await ErrorJsonHandler.ProcessRequest(context, _errorLog);
                        break;
                    case "rss":
                        await ErrorRssHandler.ProcessRequest(context, _errorLog, elmahRoot);
                        break;
                    case "digestrss":
                        await ErrorDigestRssHandler.ProcessRequest(context, _errorLog, elmahRoot);
                        return;
                    case "download":
                        await ErrorLogDownloadHandler.ProcessRequestAsync(_errorLog, context);
                        return;
                    case "test":
                        throw new TestException();
                    default:
                        await ErrorResourceHandler.ProcessRequest(context, resource, elmahRoot);
                        break;
                }
            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elmah request processing error");
            }
        }
    }
}