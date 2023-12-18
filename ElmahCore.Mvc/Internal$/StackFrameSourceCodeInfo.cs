namespace ElmahCore.Mvc
{
    public class StackFrameSourceCodeInfo
    {
        public string Function { get; set; } = default!;
        public string File { get; set; } = default!;
        public int Line { get; set; }
        public int PreContextLine { get; set; }
        public string PreContextCode { get; set; } = default!;
        public string ContextCode { get; set; } = default!;
        public string PostContextCode { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string? FileName { get; set; }
    }
}