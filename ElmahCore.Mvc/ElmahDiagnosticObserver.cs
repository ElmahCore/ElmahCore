using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace ElmahCore.Mvc
{
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
            if (value.Name != "SqlClientDiagnosticListener") return;

            var subscription = value.Subscribe(new ElmahDiagnosticSqlObserver(_provider));
            _subscriptions.Add(subscription);
        }
    }

    public class ElmahDiagnosticSqlObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly IServiceProvider _provider;

        public ElmahDiagnosticSqlObserver(IServiceProvider provider)
        {
            _provider = provider;
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (value.Value == null || value.Key != "Microsoft.Data.SqlClient.WriteCommandBefore" &&
                value.Key != "Microsoft.Data.SqlClient.WriteCommandAfter") return;

            ElmahLogFeature sqlLog;
            try
            {
                sqlLog = _provider.GetService<IHttpContextAccessor>()?.HttpContext?.Features.Get<ElmahLogFeature>();
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            if (sqlLog == null) return;

            var id = GetValueFromAnonymousType<Guid>(value.Value, "OperationId");

            switch (value.Key)
            {
                case "Microsoft.Data.SqlClient.WriteCommandAfter":
                    sqlLog.SetSqlDuration(id);
                    break;
                case "Microsoft.Data.SqlClient.WriteCommandBefore":
                {
                    var cmd = GetValueFromAnonymousType<SqlCommand>(value.Value, "Command");

                    var query = cmd.Parameters.Cast<SqlParameter>().Aggregate(cmd.CommandText, (current, p) =>
                        current.Replace(p.ParameterName, p.Value?.ToString()));

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

        private static T GetValueFromAnonymousType<T>(object dataItem, string itemKey)
        {
            var type = dataItem.GetType();
            var value = (T) type.GetProperty(itemKey)?.GetValue(dataItem, null);
            return value;
        }
    }
}
