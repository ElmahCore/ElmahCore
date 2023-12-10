using System;
using System.Collections.Generic;
using System.Linq;

namespace ElmahCore.Mvc
{
    internal class ElmahLogFeature : IElmahLogFeature
    {
        private readonly Dictionary<Guid, ElmahLogSqlEntry> _map = new();
        private readonly List<ElmahLogMessageEntry> _logs = new();
        private readonly List<ElmahLogParameters> _params = new();

        public IReadOnlyCollection<ElmahLogMessageEntry> Log => _logs;
        public IReadOnlyCollection<ElmahLogParameters> Params => _params;
        public IReadOnlyCollection<ElmahLogSqlEntry> LogSql => _map.Values.OrderBy(i => i.TimeStamp).ToList();

        public void AddMessage(ElmahLogMessageEntry entry)
        {
            _logs.Add(entry);
        }

        public void AddSql(Guid id, ElmahLogSqlEntry entry)
        {
            _map.Add(id, entry);
        }

        public void SetSqlDuration(Guid id)
        {
            if (!_map.ContainsKey(id))
            {
                return;
            }

            var data = _map[id];
            data.DurationMs = (int)Math.Round((DateTime.Now - data.TimeStamp).TotalMilliseconds);
        }

        public void LogParameters((string name, object value)[] list, string typeName, string memberName,
            string file, int line)
        {
            _params.Add(new ElmahLogParameters(DateTime.Now, list, typeName, memberName, file, line));
        }
    }
}
