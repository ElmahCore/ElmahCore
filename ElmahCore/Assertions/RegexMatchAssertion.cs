using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ElmahCore.Assertions;

#region Imports

#endregion

/// <summary>
///     An assertion implementation whose test is based on whether
///     the result of an input expression evaluated against a context
///     matches a regular expression pattern or not.
/// </summary>
internal sealed class RegexMatchAssertion : DataBoundAssertion
{
    public RegexMatchAssertion(IContextExpression source, Regex regex) :
        base(source)
    {
        RegexObject = regex ?? throw new ArgumentNullException(nameof(regex));
    }

    public IContextExpression Source => Expression;

    // ReSharper disable once MemberCanBePrivate.Global
    public Regex RegexObject { get; }

    protected override bool TestResult(object? result)
    {
        return TestResultMatch(Convert.ToString(result, CultureInfo.InvariantCulture)!);
    }

    private bool TestResultMatch(string result)
    {
        return RegexObject.Match(result).Success;
    }
}