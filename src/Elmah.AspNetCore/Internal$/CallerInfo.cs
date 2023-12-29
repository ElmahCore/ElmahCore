using System;
using System.Runtime.CompilerServices;

namespace Elmah.AspNetCore;

[Serializable]
internal sealed class CallerInfo
{
    public static readonly CallerInfo Empty = new CallerInfo(null);
    private readonly string? _filePath;

    private readonly string? _memberName;

    public CallerInfo(
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        _memberName = memberName;
        _filePath = filePath;
        LineNumber = lineNumber;
    }

    public string MemberName => _memberName ?? string.Empty;
    public string FilePath => _filePath ?? string.Empty;
    public int LineNumber { get; }

    public bool IsEmpty => 0 == MemberName.Length
                           && 0 == FilePath.Length
                           && 0 == LineNumber;

    public override string ToString()
    {
        return $"{MemberName ?? "<?member>"}@{FilePath ?? "<?filename>"}:{LineNumber}";
    }
}