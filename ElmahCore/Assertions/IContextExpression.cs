namespace ElmahCore.Assertions
{
    internal interface IContextExpression
    {
        object? Evaluate(object context);
    }
}