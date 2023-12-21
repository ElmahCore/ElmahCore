using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ElmahCore.Mvc;

internal static class StackTraceParser
{
    private static readonly Regex Regex = new Regex(@"
            ^
            \s*
            \w+ \s+ 
            (?<frame>
                (?<type> .+ ) \.
                (?<method> .+? ) \s*
                (?<params>  \( ( \s* \)
                               |        (?<pt> .+?) \s+ (?<pn> .+?) 
                                 (, \s* (?<pt> .+?) \s+ (?<pn> .+?) )* \) ) )
                ( \s+
                    ( # Microsoft .NET stack traces
                    \w+ \s+ 
                    (?<file> [a-z] \: .+? ) 
                    \: \w+ \s+ 
                    (?<line> [0-9]+ ) \p{P}?  
                    | # Mono stack traces
                    \[0x[0-9a-f]+\] \s+ \w+ \s+ 
                    <(?<file> [^>]+ )>
                    :(?<line> [0-9]+ )
                    )
                )?
            )
            \s* 
            $",
        RegexOptions.IgnoreCase
        | RegexOptions.Multiline
        | RegexOptions.ExplicitCapture
        | RegexOptions.CultureInvariant
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled);

    public static IEnumerable<TFrame> Parse<TToken, TMethod, TParameters, TParameter, TSourceLocation, TFrame>(
        string text,
        Func<int, int, string, TToken> tokenSelector,
        Func<TToken, TToken, TMethod> methodSelector,
        Func<TToken, TToken, TParameter> parameterSelector,
        Func<TToken, IEnumerable<TParameter>, TParameters> parametersSelector,
        Func<TToken, TToken, TToken, TToken, TSourceLocation> sourceLocationSelector,
        Func<TToken, TMethod, TParameters, TSourceLocation, TFrame> selector)
    {
        return from Match m in Regex.Matches(text)
            select m.Groups
            into groups
            let pt = groups["pt"].Captures
            let pn = groups["pn"].Captures
            select selector(Token(groups["frame"], tokenSelector),
                methodSelector(
                    Token(groups["type"], tokenSelector),
                    Token(groups["method"], tokenSelector)),
                parametersSelector(
                    Token(groups["params"], tokenSelector),
                    from i in Enumerable.Range(0, pt.Count)
                    select parameterSelector(Token(pt[i], tokenSelector),
                        Token(pn[i], tokenSelector))),
                sourceLocationSelector(Token(groups["file"], tokenSelector),
                    Token(groups["line"], tokenSelector),
                    Token(groups["method"], tokenSelector),
                    Token(groups["type"], tokenSelector)));
    }

    private static T Token<T>(Capture capture, Func<int, int, string, T> tokenSelector)
    {
        return tokenSelector(capture.Index, capture.Length, capture.Value);
    }
}