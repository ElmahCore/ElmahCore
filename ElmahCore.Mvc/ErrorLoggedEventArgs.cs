using System;

namespace ElmahCore.Mvc
{
    [Serializable]
    public sealed class ErrorLoggedEventArgs : EventArgs
    {
        private readonly ErrorLogEntry _entry;

        public ErrorLoggedEventArgs(ErrorLogEntry entry)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
        }

        public ErrorLogEntry Entry => _entry;
    }
}