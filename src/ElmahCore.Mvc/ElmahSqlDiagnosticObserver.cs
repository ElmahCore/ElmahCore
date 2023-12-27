using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;

namespace ElmahCore.Mvc;

internal sealed class ElmahSqlDiagnosticObserver : IObserver<DiagnosticListener>, IDisposable
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly List<IDisposable> _subscriptions = new();

    public ElmahSqlDiagnosticObserver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    [DiagnosticName("System.Data.SqlClient.WriteCommandAfter")]
    public void OnSystemCommandAfter(Guid operationId) => this.OnCommandEnd(operationId);

    [DiagnosticName("Microsoft.Data.SqlClient.WriteCommandAfter")]
    public void OnMicrosoftCommandAfter(Guid operationId) => this.OnCommandEnd(operationId);

    [DiagnosticName("System.Data.SqlClient.WriteCommandBefore")]
    public void OnSystemCommandBefore(Guid operationId, DbCommand command) => this.OnCommandStart(operationId, command);

    [DiagnosticName("Microsoft.Data.SqlClient.WriteCommandBefore")]
    public void OnMicrosoftCommandBefore(Guid operationId, DbCommand command) => this.OnCommandStart(operationId, command);

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
            TimerStart = Stopwatch.GetTimestamp(),
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
            case DbType.Guid:
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
