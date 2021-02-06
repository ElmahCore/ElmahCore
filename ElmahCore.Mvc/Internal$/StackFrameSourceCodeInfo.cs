namespace ElmahCore.Mvc
{
    public class StackFrameSourceCodeInfo
    {
        public string Function { get; set; }
        public string File { get; set; }
        public int Line { get; set; }
        public int PreContextLine { get; set; }
        public string PreContextCode { get; set; }
        public string ContextCode { get; set; }
        public string PostContextCode { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
    }
}