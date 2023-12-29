namespace Elmah.AspNetCore.Assertions;

/// <summary>
///     An static assertion implementation that always evaluates to
///     a preset value.
/// </summary>
internal sealed class StaticAssertion : IAssertion
{
    // ReSharper disable once UnusedMember.Global
    public static readonly StaticAssertion True = new StaticAssertion(true);
    public static readonly StaticAssertion False = new StaticAssertion(false);

    private readonly bool _value;

    private StaticAssertion(bool value)
    {
        _value = value;
    }

    public bool Test(AssertionHelperContext context)
    {
        return _value;
    }
}