using System;
using System.Collections.Generic;

namespace ElmahCore
{
    public class ElmahLogParamEntry
    {
        public ElmahLogParamEntry(DateTime timeStamp, KeyValuePair<string,string>[] @params, string? typeName, string memberName,
            string file, int line)
        {
            Params = @params;
            TypeName = typeName;
            MemberName = memberName;
            File = file;
            Line = line;
            TimeStamp = timeStamp;
        }

        public DateTime TimeStamp { get; }
        public KeyValuePair<string,string>[] Params { get; }
        public string? TypeName { get; }
        public string MemberName { get; }
        public string File { get; }
        public int Line { get; }
    }
}