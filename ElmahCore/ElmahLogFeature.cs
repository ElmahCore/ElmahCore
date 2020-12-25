using System.Collections.Generic;

namespace ElmahCore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class ElmahLogFeature    
    {
        public readonly List<ElmahLogMessageEntry> Log = new List<ElmahLogMessageEntry>();
        public void AddMessage(ElmahLogMessageEntry entry)
        {
            Log.Add(entry);
        }
    }
}