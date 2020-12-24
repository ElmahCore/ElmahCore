using System;
using System.Collections.Generic;
using System.Linq;

namespace ElmahCore
{
   

    public delegate void ExceptionFilterEventHandler(object sender, ExceptionFilterEventArgs args);

    [ Serializable ]
    public sealed class ExceptionFilterEventArgs : EventArgs
    {
        private readonly List<string> _notifiers = new List<string>();
        internal IEnumerable<string> DismissedNotifiers => _notifiers;

        public ExceptionFilterEventArgs(Exception e, object context)
        {
	        Exception = e ?? throw new ArgumentNullException(nameof(e));
            Context = context;
        }

		public Exception Exception { get; }

		[field: NonSerialized]
        public object Context { get; }

        public bool Dismissed { get; private set; }

		public void Dismiss()
        {
            Dismissed = true;
        }
        public void DismissForNotifiers(IEnumerable<string> notifiers)
        {
            foreach (var notifier in notifiers)
            {
                if (!_notifiers.Any(i => i.Equals(notifier, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _notifiers.Add(notifier);
                }
            }
            Dismissed = true;
        }
    }
}