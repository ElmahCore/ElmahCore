using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging.Abstractions;

namespace ElmahCore.Mvc;

internal sealed class ElmahDiagnosticObserver : IObserver<DiagnosticListener>, IDisposable
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

    public ElmahDiagnosticObserver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    [DiagnosticName("System.Data.SqlClient.WriteCommandAfter")]
    public void OnSystemCommandAfter(Guid operationId, string operation, Guid _, DbCommand command) => this.OnCommandEnd(operationId);

    [DiagnosticName("Microsoft.Data.SqlClient.WriteCommandAfter")]
    public void OnMicrosoftCommandAfter(Guid operationId, string operation, Guid _, DbCommand command) => this.OnCommandEnd(operationId);

    [DiagnosticName("System.Data.SqlClient.WriteCommandAfter")]
    public void OnSystemCommandBefore(Guid operationId, string operation, Guid _, DbCommand command) => this.OnCommandStart(operationId, command);

    [DiagnosticName("Microsoft.Data.SqlClient.WriteCommandAfter")]
    public void OnMicrosoftCommandBefore(Guid operationId, string operation, Guid _, DbCommand command) => this.OnCommandStart(operationId, command);

    public void Dispose() => this.Clear();

    public void OnCompleted() => this.Clear();

    public void OnError(Exception error)
    {
    }

    public void OnNext(DiagnosticListener value)
    {
        if (value.Name != "SqlClientDiagnosticListener")
        {
            return;
        }

        var subscription = value.SubscribeWithAdapter(this);
        _subscriptions.Add(subscription);
    }

    private IElmahLogFeature? GetElmahContext()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.Features.Get<IElmahLogFeature>();
        }
        catch (ObjectDisposedException)
        {
            return null;
        }
    }

    private void OnCommandStart(Guid id, DbCommand cmd)
    {
        var context = this.GetElmahContext();
        if (context is null)
        {
            return;
        }

        string query = cmd.CommandText;
        if (query.Contains("/* elmah */"))
        {
            return;
        }

        if (cmd.CommandType == CommandType.StoredProcedure)
        {
            query = (query + " " + string.Join(", ", cmd.Parameters
                .Cast<DbParameter>()
                .Select(p => $"{p.ParameterName}={FormatParameterValue(p)}")))
                .Trim();
        }
        else
        {
            query = cmd.Parameters
                .Cast<DbParameter>()
                .Aggregate(cmd.CommandText, (current, p) => current.Replace(p.ParameterName, $"/*{p.ParameterName}*/ {FormatParameterValue(p)}"));
        }

        context.AddSql(id, new ElmahLogSqlEntry
        {
            CommandType = cmd.CommandType.ToString(),
            SqlText = query,
            TimeStamp = DateTime.Now,
            DurationMs = 0
        });
    }

    private void OnCommandEnd(Guid id)
    {
        var context = this.GetElmahContext();
        if (context is null)
        {
            return;
        }

        context.SetSqlDuration(id);
    }

    private void Clear()
    {
        _subscriptions.ForEach(x => x.Dispose());
        _subscriptions.Clear();
    }

    private static string FormatParameterValue(DbParameter parameter)
    {
        if (parameter.Value is null || parameter.Value == DBNull.Value)
        {
            return "null";
        }

        switch (parameter.DbType)
        {
            case DbType.String:
            case DbType.AnsiString:
            case DbType.AnsiStringFixedLength:
            case DbType.StringFixedLength:
            case DbType.Xml:
                return $"'{parameter.Value}'";
            case DbType.Date:
            case DbType.Time:
            case DbType.DateTime:
            case DbType.DateTime2:
            case DbType.DateTimeOffset:
                // TODO: format these appropriately
                return $"'{parameter.Value}'";
            case DbType.Boolean:
                return (bool)parameter.Value ? "1" : "0";
            default:
                return parameter.Value.ToString()!;
        }
    }
}

internal class ElmahDiagnosticSqlObserver : IObserver<KeyValuePair<string, object?>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly HashSet<string> Events = new(new[] {
            "System.Data.SqlClient.WriteCommandAfter",
            "Microsoft.Data.SqlClient.WriteCommandAfter",
            "System.Data.SqlClient.WriteCommandBefore",
            "Microsoft.Data.SqlClient.WriteCommandBefore"}, 
        StringComparer.Ordinal);

    public ElmahDiagnosticSqlObserver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(KeyValuePair<string, object?> value)
    {
        if (value.Value is null || !Events.Contains(value.Key))
        {
            return;
        }

        IElmahLogFeature? sqlLog;
        try
        {
            sqlLog = _httpContextAccessor.HttpContext?.Features.Get<IElmahLogFeature>();
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        if (sqlLog is null)
        {
            return;
        }

        var id = GetValueFromAnonymousType<Guid>(value.Value, "OperationId");

        switch (value.Key)
        {
            case "System.Data.SqlClient.WriteCommandAfter":
            case "Microsoft.Data.SqlClient.WriteCommandAfter":
                sqlLog.SetSqlDuration(id);
                break;
            case "System.Data.SqlClient.WriteCommandBefore":
            case "Microsoft.Data.SqlClient.WriteCommandBefore":
                {
                    var cmd = GetValueFromAnonymousType<DbCommand>(value.Value, "Command");
                    if (cmd is null)
                    {
                        return;
                    }

                    string query = cmd.CommandText;
                    if (cmd.CommandType == CommandType.StoredProcedure)
                    {
                        query = (query + " " + string.Join(", ", cmd.Parameters
                            .Cast<DbParameter>()
                            .Select(p => $"{p.ParameterName}={FormatParameterValue(p)}")))
                            .Trim();
                    }
                    else
                    {
                        query = cmd.Parameters
                            .Cast<DbParameter>()
                            .Aggregate(cmd.CommandText, (current, p) => current.Replace(p.ParameterName, $"/*{p.ParameterName}*/ {FormatParameterValue(p)}"));
                    }

                    if (!query.Contains("/* elmah */"))
                    {
                        sqlLog.AddSql(id, new ElmahLogSqlEntry
                        {
                            CommandType = cmd.CommandType.ToString(),
                            SqlText = query,
                            TimeStamp = DateTime.Now,
                            DurationMs = 0
                        });
                    }

                    break;
                }
        }
    }

    public void OnCompleted()
    {
    }

    private static T? GetValueFromAnonymousType<T>(object dataItem, string itemKey)
    {
        var type = dataItem.GetType();
        var value = (T?)type.GetProperty(itemKey)?.GetValue(dataItem, null);
        return value;
    }

    private static string FormatParameterValue(DbParameter parameter)
    {
        if (parameter.Value is null || parameter.Value == DBNull.Value)
        {
            return "null";
        }

        switch (parameter.DbType)
        {
            case DbType.String:
            case DbType.AnsiString:
            case DbType.AnsiStringFixedLength:
            case DbType.StringFixedLength:
            case DbType.Xml:
                return $"'{parameter.Value}'";
            case DbType.Date:
            case DbType.Time:
            case DbType.DateTime:
            case DbType.DateTime2:
            case DbType.DateTimeOffset:
                // TODO: format these appropriately
                return $"'{parameter.Value}'";
            case DbType.Boolean:
                return (bool)parameter.Value ? "1" : "0";
            default:
                return parameter.Value.ToString()!;
        }
    }
}
