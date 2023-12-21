namespace ElmahCore.Assertions;

internal sealed class WebDataBindingExpression : IContextExpression
{
    private readonly string _expression;

    public WebDataBindingExpression(string expression)
    {
        _expression = expression;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public string Expression => _expression ?? string.Empty;

    public object? Evaluate(object context)
    {
        return DataBinder.Eval(context, Expression);
    }
}