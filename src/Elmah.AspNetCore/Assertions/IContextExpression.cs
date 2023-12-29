namespace Elmah.AspNetCore.Assertions;

internal interface IContextExpression
{
    object? Evaluate(object context);
}