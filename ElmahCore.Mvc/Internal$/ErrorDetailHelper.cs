using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace ElmahCore.Mvc
{
    internal class SourceInfo
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public string Source { get; set; }
        public int Line { get; set; }
    }

    internal static class ErrorDetailHelper
    {
        internal static Dictionary<string, StackFrameSourceCodeInfo> Cache =
            new Dictionary<string, StackFrameSourceCodeInfo>();

        // make it internal to enable unit testing
        internal static StackFrameSourceCodeInfo GetStackFrameSourceCodeInfo(string[] sourcePath, string method,
            string type, string filePath, int lineNumber)
        {
            var key = $"{method}:{type}:{filePath}:{lineNumber}";
            lock (Cache)
            {
                if (Cache.ContainsKey(key)) return Cache[key];
            }

            var stackFrame = new StackFrameSourceCodeInfo
            {
                Function = method,
                Type = type,
                File = filePath,
                FileName = Path.GetFileName(filePath),
                Line = lineNumber
            };

            if (string.IsNullOrEmpty(stackFrame.File)) return stackFrame;

            IEnumerable<string> lines = null;
            var path = GetPath(sourcePath, filePath);

            if (path != null) lines = File.ReadLines(path);

            if (lines != null) ReadFrameContent(stackFrame, lines, stackFrame.Line, stackFrame.Line);

            lock (Cache)
            {
                if (!Cache.ContainsKey(key)) Cache.Add(key, stackFrame);
            }

            return stackFrame;
        }

        private static string GetPath(string[] sourcePaths, string filePath)
        {
            sourcePaths ??= new[] {""};
            foreach (var source in sourcePaths)
            {
                var sourcePath = source;
                var split = filePath.Split(Path.DirectorySeparatorChar);
                if (source != "" && !sourcePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    sourcePath += Path.DirectorySeparatorChar;
                var curLen = 0;
                foreach (var subPath in split)
                {
                    var curPath = sourcePath + filePath.Substring(curLen);
                    if (File.Exists(curPath))
                        return curPath;
                    curLen += subPath.Length + 1;
                }
            }

            return null;
        }

        // make it internal to enable unit testing
        internal static void ReadFrameContent(
            StackFrameSourceCodeInfo frame,
            IEnumerable<string> allLines,
            int errorStartLineNumberInFile,
            int errorEndLineNumberInFile)
        {
            // Get the line boundaries in the file to be read and read all these lines at once into an array.
            var preErrorLineNumberInFile = Math.Max(errorStartLineNumberInFile - 10, 1);
            var postErrorLineNumberInFile = errorEndLineNumberInFile + 10;
            var codeBlock = allLines
                .Skip(preErrorLineNumberInFile - 1)
                .Take(postErrorLineNumberInFile - preErrorLineNumberInFile + 1)
                .ToArray();

            var numOfErrorLines = errorEndLineNumberInFile - errorStartLineNumberInFile + 1;
            var errorStartLineNumberInArray = errorStartLineNumberInFile - preErrorLineNumberInFile;

            frame.PreContextLine = preErrorLineNumberInFile;
            frame.PreContextCode = string.Join("\n", codeBlock.Take(errorStartLineNumberInArray));
            frame.ContextCode = string.Join("\n", codeBlock
                .Skip(errorStartLineNumberInArray)
                .Take(numOfErrorLines));
            frame.PostContextCode = string.Join("\n", codeBlock
                .Skip(errorStartLineNumberInArray + numOfErrorLines));
        }

        public static string MarkupStackTrace(string text, out List<SourceInfo> srcList)
        {
            Debug.Assert(text != null);

            var list = new List<SourceInfo>();
            srcList = list;

            var frames = StackTraceParser.Parse
            (
                text,
                (idx, len, txt) => new
                {
                    Index = idx,
                    End = idx + len,
                    Html = txt.Length > 0
                        ? WebUtility.HtmlEncode(txt)
                        : string.Empty
                },
                (t, m) => new
                {
                    Type = new {t.Index, t.End, Html = $"<span class='st-type'>{t.Html}</span>"},
                    Method = new {m.Index, m.End, Html = $"<span class='st-method'>{m.Html}</span>"}
                },
                (t, n) => new
                {
                    Type = new {t.Index, t.End, Html = $"<span class='st-param-type'>{t.Html}</span>"},
                    Name = new {n.Index, n.End, Html = $"<span class='st-param-name'>{n.Html}</span>"}
                },
                (p, ps) => new {List = p, Parameters = ps.ToArray()},
                (f,l) =>
                
                {
                    if (int.TryParse(l.Html, out var line))
                        list.Add(new SourceInfo
                        {
                            Source = f.Html,
                            Line = line,
                        });
                    return new
                    {
                        File = f.Html.Length > 0
                            ? new {f.Index, f.End, Html = $"<span class='st-file'>{f.Html}</span>"}
                            : null,
                        Line = l.Html.Length > 0
                            ? new {l.Index, l.End, Html = $"<span class='st-line'>{l.Html}</span>"}
                            : null
                    };
                },
                (f, tm, p, fl) =>
                    from tokens in new[]
                    {
                        new[]
                        {
                            new {f.Index, End = f.Index, Html = "<span class='st-frame'>"},
                            tm.Type,
                            tm.Method,
                            new {p.List.Index, End = p.List.Index, Html = "<span class='params'>"}
                        },
                        from pe in p.Parameters
                        from e in new[] {pe.Type, pe.Name}
                        select e,
                        new[]
                        {
                            new {Index = p.List.End, p.List.End, Html = "</span>"},
                            fl.File,
                            fl.Line,
                            new {Index = f.End, f.End, Html = "</span>"}
                        }
                    }
                    from token in tokens
                    where token != null
                    select token
            );

            var markups =
                from token in Enumerable.Repeat(new {Index = 0, End = 0, Html = string.Empty}, 1)
                    .Concat(from tokens in frames from token in tokens select token)
                    .Pairwise((prev, curr) => new {Previous = prev, Current = curr})
                from m in new object[]
                {
                    text.Substring(token.Previous.End, token.Current.Index - token.Previous.End),
                    token.Current.Html
                }
                select m.ToString()
                into m
                where m.Length > 0
                select m;

            return string.Join(string.Empty, markups);
        }
    }
}