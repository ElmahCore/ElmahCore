using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Elmah.AspNetCore.Assertions;

/// <summary>
///     An assertion implementation whose test is based on whether
///     the result of an input expression evaluated against a context
///     matches a regular expression pattern or not.
/// </summary>
internal class ComparisonAssertion : DataBoundAssertion
{
    private readonly Predicate<int> _predicate;

    public ComparisonAssertion(Predicate<int> predicate, IContextExpression source, TypeCode type, string value) :
        base(source)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

        if (type == TypeCode.DBNull
            || type == TypeCode.Empty
            || type == TypeCode.Object)
        {
            var message = $"The {type} value type is invalid for a comparison.";
            throw new ArgumentException(message, nameof(type));
        }

        //
        // Convert the expected value to the comparison type and 
        // save it as a field.
        //

        ExpectedValue = Convert.ChangeType(value, type /*, FIXME CultureInfo.InvariantCulture */);
    }

    public IContextExpression Source => Expression;

    public object ExpectedValue { get; }

    public override bool Test(AssertionHelperContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return ExpectedValue != null && base.Test(context);
    }

    protected override bool TestResult(object? result)
    {
        if (result == null)
        {
            return false;
        }

        var right = ExpectedValue as IComparable;

        if (right == null)
        {
            return false;
        }

        return Convert.ChangeType(result, right.GetType(), CultureInfo.InvariantCulture) is IComparable left &&
               TestComparison(left, right);
    }

    protected bool TestComparison(IComparable left, IComparable right)
    {
        if (left == null)
        {
            throw new ArgumentNullException(nameof(left));
        }

        return _predicate(left.CompareTo(right));
    }
}