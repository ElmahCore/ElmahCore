using System;

namespace Elmah.AspNetCore.Assertions;

internal sealed class UnaryNotAssertion : IAssertion
{
    public UnaryNotAssertion(IAssertion operand)
    {
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public IAssertion Operand { get; }

    public bool Test(AssertionHelperContext context)
    {
        return !Operand.Test(context);
    }
}