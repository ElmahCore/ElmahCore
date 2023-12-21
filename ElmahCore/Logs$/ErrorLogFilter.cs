using System;
using System.Text.RegularExpressions;

namespace ElmahCore;

public class ErrorLogFilter
{
    public ErrorLogFilter(string property, ErrorLogPropertyType propertyType, ErrorLogFilterCondition condition, string value)
    {
        Property = property;
        PropertyType = propertyType;
        Condition = condition;
        Value = value;
    }

    public string Property { get; }
    public ErrorLogPropertyType PropertyType { get; }
    public ErrorLogFilterCondition Condition { get; }
    public string Value { get; }

    public DateTime? GetValueAsDateTime()
    {
        if (string.IsNullOrEmpty(Value))
        {
            return null;
        }

        return DateTime.ParseExact(Value, Value.Length == 10 ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture);
    }

    public static ErrorLogFilter? Parse(string str)
    {
        var regex = new Regex(@"([^\s]*)\s+([^\s]*)\s+(.*)");
        var match = regex.Match(str);
        if (!match.Success)
        {
            return null;
        }

        var property = match.Groups[1].Value;
        var condition = match.Groups[2].Value;
        var value = match.Groups[3].Value;

        var propertyType = property switch
        {
            "date-time" => ErrorLogPropertyType.DateTime,
            _ => ErrorLogPropertyType.String
        };

        var con = condition switch
        {
            "=" => ErrorLogFilterCondition.Equals,
            "!=" => ErrorLogFilterCondition.NotEquals,
            "~" => ErrorLogFilterCondition.Contains,
            "!~" => ErrorLogFilterCondition.DoesNotContain,
            _ => throw new NotSupportedException($"Condition {condition} not supported")
        };

        return new ErrorLogFilter(property, propertyType, con, value);
    }
}