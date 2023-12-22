using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ElmahCore.Mvc;

internal sealed class ElmahDiagnosticObserver : IObserver<DiagnosticListener>
{
    private readonly IServiceProvider _provider;
    private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

    public ElmahDiagnosticObserver(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void OnCompleted()
    {
        _subscriptions.ForEach(x => x.Dispose());
        _subscriptions.Clear();
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(DiagnosticListener value)
    {
        if (value.Name != "SqlClientDiagnosticListener")
        {
            return;
        }

        var subscription = value.Subscribe(new ElmahDiagnosticSqlObserver(_provider));
        _subscriptions.Add(subscription);
    }
}

public class ElmahDiagnosticSqlObserver : IObserver<KeyValuePair<string, object?>>
{
    private readonly IServiceProvider _provider;

    public ElmahDiagnosticSqlObserver(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(KeyValuePair<string, object?> value)
    {
        if (value.Value == null || value.Key != "System.Data.SqlClient.WriteCommandBefore" &&
            value.Key != "System.Data.SqlClient.WriteCommandAfter")
        {
            return;
        }

        IElmahLogFeature? sqlLog;
        try
        {
            sqlLog = _provider.GetService<IHttpContextAccessor>()?.HttpContext?.Features.Get<IElmahLogFeature>();
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        if (sqlLog == null)
        {
            return;
        }

        var id = GetValueFromAnonymousType<Guid>(value.Value, "OperationId");

        switch (value.Key)
        {
            case "System.Data.SqlClient.WriteCommandAfter":
                sqlLog.SetSqlDuration(id);
                break;
            case "System.Data.SqlClient.WriteCommandBefore":
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
                return $"'{parameter.Value}'";
            default:
                return parameter.Value.ToString()!;
        }
    }
}
