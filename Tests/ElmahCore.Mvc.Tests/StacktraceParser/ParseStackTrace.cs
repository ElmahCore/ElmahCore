using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentAssertions;
using Xunit;

namespace ElmahCore.Mvc.Tests.StacktraceParser
{
    public class ParseStackTrace
    {
        private List<SourceInfo> list = new List<SourceInfo>();

        [Fact]
        public void CanParseStackTraceString()
        {
            var frames = StackTraceParser.Parse
            (
                GetTestStackStringIssue158_HighCPULoop(),
                (idx, len, txt) => new
                {
                    Index = idx,
                    End = idx + len,
                    Html = txt.Length > 0
                        ? WebUtility.HtmlEncode(txt)
                        : string.Empty
                },
                (t, m) => new
                {
                    Type = new { t.Index, t.End, Html = $"<span class='st-type'>{t.Html}</span>" },
                    Method = new { m.Index, m.End, Html = $"<span class='st-method'>{m.Html}</span>" }
                },
                (t, n) => new
                {
                    Type = new { t.Index, t.End, Html = $"<span class='st-param-type'>{t.Html}</span>" },
                    Name = new { n.Index, n.End, Html = $"<span class='st-param-name'>{n.Html}</span>" }
                },
                (p, ps) => new { List = p, Parameters = ps.ToArray() },
                (f, l) =>

                {
                    if (int.TryParse(l.Html, out var line))
                        list.Add(new SourceInfo
                        {
                            Source = f.Html,
                            Line = line,
                        });
                    return new
                    {
                        File = f.Html.Length > 0
                            ? new { f.Index, f.End, Html = $"<span class='st-file'>{f.Html}</span>" }
                            : null,
                        Line = l.Html.Length > 0
                            ? new { l.Index, l.End, Html = $"<span class='st-line'>{l.Html}</span>" }
                            : null
                    };
                },
                (f, tm, p, fl) =>
                    from tokens in new[]
                    {
                        new[]
                        {
                            new { f.Index, End = f.Index, Html = "<span class='st-frame'>" },
                            tm.Type,
                            tm.Method,
                            new { p.List.Index, End = p.List.Index, Html = "<span class='params'>" }
                        },
                        from pe in p.Parameters
                        from e in new[] { pe.Type, pe.Name }
                        select e,
                        new[]
                        {
                            new { Index = p.List.End, p.List.End, Html = "</span>" },
                            fl.File,
                            fl.Line,
                            new { Index = f.End, f.End, Html = "</span>" }
                        }
                    }
                    from token in tokens
                    where token != null
                    select token
            );

            frames.Should().NotBeNull();
        }

        public string GetTestStackStringIssue158_HighCPULoop()
        {
            return @"
# caller: @C:\Sources\ElmahCore\ElmahCore\ElmahCore\Internal$\CallerInfo.cs:9
MySqlConnector.MySqlException (0x80004005): Column 'OrganizationNodeName' cannot be null
   at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 43
   at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 132
   at MySqlConnector.MySqlDataReader.CreateAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 466
   at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(IReadOnlyList`1 commands, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
   at MySqlConnector.MySqlCommand.ExecuteNonQueryAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 296
   at MySqlConnector.MySqlCommand.ExecuteNonQuery() in /_/src/MySqlConnector/MySqlCommand.cs:line 107
   at PMIS.DAL.MySQL.OrganizationNodeDAL.UpdateName(String organizationId, String nodeId, String name) in C:\git2017\PMIS.DAL\MySQL\OrganizationNodeDal.cs:line 135
   at PMIS.Service.OrganizationNodeService.UpdateName(Account user, String nodeId, String newName) in C:\git2017\PMIS.Service\OrganizationNodeService.cs:line 258
   at PMIS.Controllers.OrganizationNodeController.UpdateName(String nodeId, String name, String bindProjectId) in C:\git2017\PMISCore\Controllers\OrganizationNodeController.cs:line 110
   at lambda_method(Closure , Object )
   at Microsoft.Extensions.Internal.ObjectMethodExecutorAwaitable.Awaiter.GetResult()
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfActionResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
--- End of stack trace from previous location where exception was thrown ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|24_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
--- End of stack trace from previous location where exception was thrown ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|6_0(Endpoint endpoint, Task requestTask, ILogger logger)
   at ElmahCore.Mvc.ErrorLogMiddleware.InvokeAsync(HttpContext context)";
        }
    }
}