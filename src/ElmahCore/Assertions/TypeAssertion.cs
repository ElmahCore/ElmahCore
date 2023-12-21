using System;
using System.Diagnostics;

// ReSharper disable MemberCanBePrivate.Global

namespace ElmahCore.Assertions;

/// <summary>
///     An assertion implementation whose test is based on whether
///     the result of an input expression evaluated against a context
///     matches a regular expression pattern or not.
/// </summary>
internal sealed class TypeAssertion : DataBoundAssertion
{
    public TypeAssertion(IContextExpression source, Type expectedType, bool byCompatibility) :
        base(MaskNullExpression(source))
    {
        if (expectedType == null)
        {
            throw new ArgumentNullException(nameof(expectedType));
        }

        if (expectedType.IsInterface || expectedType.IsClass && expectedType.IsAbstract)
        {
            //
            // Interfaces and abstract classes will always have an 
            // ancestral relationship.
            //

            byCompatibility = true;
        }

        ExpectedType = expectedType;
        ByCompatibility = byCompatibility;
    }

    public IContextExpression Source => Expression;

    public Type ExpectedType { get; }

    public bool ByCompatibility { get; }

    public override bool Test(AssertionHelperContext context)
    {
        return context == null
            ? throw new ArgumentNullException(nameof(context))
            : ExpectedType != null && base.Test(context);
    }

    protected override bool TestResult(object? result)
    {
        if (result == null)
        {
            return false;
        }

        var resultType = result.GetType();
        var expectedType = ExpectedType;

        Debug.Assert(expectedType != null);

        return ByCompatibility ? expectedType.IsAssignableFrom(resultType) : expectedType == resultType;
    }

    private static IContextExpression MaskNullExpression(IContextExpression expression)
    {
        return expression ?? new DelegatedContextExpression(EvaluateToException);
    }

    private static object? EvaluateToException(object context)
    {
        return context is ExceptionFilterEventArgs args ? args.Exception : DataBinder.Eval(context, "Exception");
    }
}