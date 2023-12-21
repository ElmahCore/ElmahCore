using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace ElmahCore.Assertions;

/// <summary>
///     Provides data expression evaluation facilites similar to
///     <see cref="DataBinder" /> in ASP.NET.
/// </summary>
internal static class DataBinder
{
    private static readonly Regex ExpressionRegex;

    static DataBinder()
    {
        const string index = @"(?<i>[0-9]+ | (?<q>['""]).+?\k<q>)";
        ExpressionRegex = new Regex(@"^(?<a> \.? ( [a-z_][a-z0-9_]* 
                                                     | \[ " + index + @" \]
                                                     | \( " + index + @" \) ) )*$",
            RegexOptions.IgnorePatternWhitespace
            | RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant);
    }

    public static object? Eval(object container, string? expression)
    {
        return Eval(container, expression, false);
    }

    public static object? Eval(object container, string? expression, bool strict)
    {
        return Compile(expression, strict)(container);
    }

    public static Func<object?, object?> Compile(string? expression)
    {
        return Compile(expression, false);
    }

    public static Func<object?, object?> Compile(string? expression, bool strict)
    {
        return obj => Parse(expression, strict).Aggregate(obj, (node, b) => b(node));
    }

    public static IEnumerable<Func<object?, object?>> Parse(string? expression)
    {
        return Parse(expression, false);
    }

    public static IEnumerable<Func<object?, object?>> Parse(string? expression, bool strict)
    {
        var mp = strict
            ? (obj, name) => throw new DataBindingException(
                $@"DataBinding: '{obj.GetType()}' does not contain a property with the name '{name}'.")
            : (Func<object, string, object>?) null;
        var mi = strict
            ? (obj, index) => throw new DataBindingException(
                $@"DataBinding: '{obj.GetType()}' does not allow indexed access.")
            : (Func<object, object, object>?) null;

        var binders = Parse(expression, p => PassThru((object? obj) => GetProperty(obj!, p, mp)),
            i => PassThru((object? obj) => GetIndex(obj!, i, mi)));

        var combinator = strict
            ? new Func<Func<object?, object?>, Func<object?, object?>>(b => b)
            : Optionalize;

        return strict ? binders : binders.Select(combinator);
    }

    private static Func<T, TResult> PassThru<T, TResult>(Func<T, TResult> f)
    {
        return f;
    }

    private static Func<object?, object?> Optionalize(Func<object?, object?> binder)
    {
        return obj => obj == null || Convert.IsDBNull(obj) ? null : binder(obj);
    }

    private static object? GetProperty(object obj, string name, Func<object, string, object>? missingSelector)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        var property = TypeDescriptor.GetProperties(obj).Find(name, true);
        return property != null
            ? property.GetValue(obj)
            : missingSelector?.Invoke(obj, name);
    }

    private static object? GetIndex(object obj, object index, Func<object, object, object>? missingSelector)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (index == null)
        {
            throw new ArgumentNullException(nameof(index));
        }

        var isIntegralIndex = index is int;

        if (obj is Array array && isIntegralIndex)
        {
            return array.GetValue((int) index);
        }

        if (obj is IList list && isIntegralIndex)
        {
            return list[(int) index];
        }

        var property = FindIndexerProperty(obj.GetType(), index.GetType());

        return property != null
            ? property.GetValue(obj, new[] {index})
            : missingSelector?.Invoke(obj, index);
    }

    // TODO Consider as Type extension method
    private static PropertyInfo? FindIndexerProperty(Type type, params Type[] types)
    {
        Debug.Assert(type != null);
        Debug.Assert(types != null);

        return type.GetProperty(TryGetDefaultMemberName(type) ?? "Item",
            BindingFlags.Public | BindingFlags.Instance,
            null, null, types, null);
    }

    // TODO Consider as Type extension method
    private static string? TryGetDefaultMemberName(Type type)
    {
        Debug.Assert(type != null);

        var attribute = (DefaultMemberAttribute?) Attribute.GetCustomAttribute(type, typeof(DefaultMemberAttribute));
        return attribute != null && !string.IsNullOrEmpty(attribute.MemberName)
            ? attribute.MemberName
            : null;
    }

    public static IEnumerable<T> Parse<T>(string? expression,
        Func<string, T> propertySelector,
        Func<object, T> indexSelector)
    {
        if (propertySelector == null)
        {
            throw new ArgumentNullException(nameof(propertySelector));
        }

        if (indexSelector == null)
        {
            throw new ArgumentNullException(nameof(indexSelector));
        }

        expression = (expression ?? string.Empty).Trim();
        if (expression.Length == 0)
        {
            yield break;
        }

        var match = ExpressionRegex.Match(expression);
        if (!match.Success)
        {
            throw new FormatException(null);
        }

        var accessors =
            from Capture c in match.Groups["a"].Captures
            select c.Value.TrimStart('.')
            into text
            let isIndexer = text[0] == '[' || text[0] == '('
            select new {Text = text, IsIndexer = isIndexer};

        var indexes =
            from Capture c in match.Groups["i"].Captures
            select c.Value
            into text
            select text[0] == '\'' || text[0] == '\"'
                ? (object) text.Substring(1, text.Length - 2)
                : int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);

        using var i = indexes.GetEnumerator();
        foreach (var a in accessors)
        {
            if (a.IsIndexer)
            {
                i.MoveNext();
            }

            yield return a.IsIndexer ? indexSelector(i.Current) : propertySelector(a.Text);
        }
    }
}

[Serializable]
internal class DataBindingException : Exception
{
    public DataBindingException()
    {
    }

    public DataBindingException(string message) :
        // ReSharper disable once IntroduceOptionalParameters.Global
        this(message, null)
    {
    }

    public DataBindingException(string message, Exception? inner) :
        base(message, inner)
    {
    }

    protected DataBindingException(SerializationInfo info, StreamingContext context) :
        base(info, context)
    {
    }
}