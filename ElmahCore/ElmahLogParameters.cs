using System;

namespace ElmahCore;

public class ElmahLogParameters
{
    public ElmahLogParameters(DateTime timeStamp, (string name, object value)[] @params, 
        string typeName, string memberName,
        string file, int line)
    {
        TimeStamp = timeStamp;
        Params = @params;
        TypeName = typeName;
        MemberName = memberName;
        File = file;
        Line = line;
    }
    public DateTime TimeStamp { get; }
    public (string name, object value)[] Params { get; }
    public string TypeName { get; }
    public string MemberName { get; }
    public string File { get; }
    public int Line { get; }
}