using System;

namespace Elmah.AspNetCore.Assertions;

internal static class ComparisonResults
{
    public static readonly Predicate<int> Equal = result => result == 0;
    public static readonly Predicate<int> Lesser = result => result < 0;
    public static readonly Predicate<int> LesserOrEqual = result => result <= 0;
    public static readonly Predicate<int> Greater = result => result > 0;
    public static readonly Predicate<int> GreaterOrEqual = result => result >= 0;
}