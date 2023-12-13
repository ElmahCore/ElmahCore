using System;
using System.Collections.Generic;

namespace ElmahCore
{
    internal static class ErrorLogFilterHelper
    {
        public static bool IsMatched(ErrorLogEntry entry, string? searchText, IEnumerable<ErrorLogFilter> filters)
        {
            var isMatched = DoFilter(entry, filters);
            if (isMatched)
            {
                isMatched = DoSearch(entry, searchText);   
            }

            return isMatched;
        }

        private static bool DoSearch(ErrorLogEntry entry, string? searchText)
        {
            searchText = ("" + searchText).Trim().ToLower();
            if (searchText == string.Empty) return true;
            
            foreach (var filterFunction in FilterFunctions.Values)
            {
                var value = filterFunction(entry);
                if (!(value is string stringValue)) continue;

                if (stringValue.Trim().ToLower().Contains(searchText)) return true;
            }

            return false;
        }

        private static bool DoFilter(ErrorLogEntry entry, IEnumerable<ErrorLogFilter> filters)
        {
            foreach (var filter in filters)
            {
                var isMatched = filter.PropertyType switch
                {
                    ErrorLogPropertyType.DateTime => Check(entry, filter, filter.GetValueAsDateTime(),
                        DateTimeConditions),
                    ErrorLogPropertyType.String => Check(entry, filter, filter.Value, StringConditions),
                    _ => throw new NotSupportedException($"ErrorLogPropertyType {filter.PropertyType} not supported")
                };
                if (!isMatched) return false;
            }

            return true;
        }

        private static bool Check<T>(ErrorLogEntry entry, ErrorLogFilter filter, T value,
            IReadOnlyDictionary<ErrorLogFilterCondition, Func<T, T, bool>> conditions)
        {
            var func = FindFunc(filter.Property);
            var condition = FindCondition(filter.Condition, conditions);

            var entityValue = func(entry);
            return condition((T)entityValue, value);
        }

        private static Func<ErrorLogEntry, object> FindFunc(string property)
        {
            if (!FilterFunctions.TryGetValue(property, out var func))
                throw new NotSupportedException($"Property {property} not supported");

            return func;
        }
        
        private static Func<T, T, bool> FindCondition<T>(ErrorLogFilterCondition condition, IReadOnlyDictionary<ErrorLogFilterCondition, Func<T, T, bool>> conditions)
        {
            if (!conditions.TryGetValue(condition, out var cond))
                throw new NotSupportedException($"Condition {condition} not supported");

            return cond;
        }

        private static readonly Dictionary<string, Func<ErrorLogEntry, object>> FilterFunctions = new Dictionary<string, Func<ErrorLogEntry, object>>
        {
            { "application", e => e.Error.ApplicationName },
            { "body", e => e.Error.Body },
            { "client", e => e.Error.ServerVariables["Connection_RemoteIpAddress"] },
            { "date-time", e => e.Error.Time },
            { "details", e => e.Error.Detail },
            { "host", e => e.Error.ServerVariables["Header_host"] },
            { "message", e => e.Error.Message },
            { "method", e => e.Error.ServerVariables["Header_:method"] },
            { "source", e => e.Error.Source },
            { "status-code", e => e.Error.StatusCode.ToString() },
            { "type", e => e.Error.Type },
            { "url", e => e.Error.ServerVariables["PathBase"] + e.Error.ServerVariables["Path"] },
            { "user", e => e.Error.User }
        };

        private static readonly Dictionary<ErrorLogFilterCondition, Func<string, string, bool>> StringConditions =
            new Dictionary<ErrorLogFilterCondition, Func<string, string, bool>>
            {
                { ErrorLogFilterCondition.Equals, (e, f) => e == f },
                { ErrorLogFilterCondition.NotEquals, (e, f) => e != f },
                { ErrorLogFilterCondition.Contains, (e, f) => e?.Contains(f) ?? false },
                { ErrorLogFilterCondition.DoesNotContain, (e, f) => !e?.Contains(f) ?? true },
            };
        
        private static readonly Dictionary<ErrorLogFilterCondition, Func<DateTime?, DateTime?, bool>> DateTimeConditions =
            new Dictionary<ErrorLogFilterCondition, Func<DateTime?, DateTime?, bool>>
            {
                { ErrorLogFilterCondition.Equals, (e, f) => RemoveMs(e) == RemoveMs(f) },
                { ErrorLogFilterCondition.NotEquals, (e, f) => RemoveMs(e) != RemoveMs(f) },
                { ErrorLogFilterCondition.Contains, (e, f) => e?.Date == f?.Date },
                { ErrorLogFilterCondition.DoesNotContain, (e, f) => e?.Date != f?.Date },
            };

        private static DateTime? RemoveMs(DateTime? dateTime)
        {
            return dateTime?.AddTicks( - (dateTime.Value.Ticks % TimeSpan.TicksPerSecond));
        }
    }
}