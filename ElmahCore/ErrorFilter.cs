using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ElmahCore.Assertions;

namespace ElmahCore
{
    /// <summary>
    /// HTTP module implementation that logs unhanded exceptions in an
    /// ASP.NET Web application to an error log.
    /// </summary>

    // ReSharper disable once UnusedMember.Global
    internal class ErrorFilter : IErrorFilter, INotifierProvider
    {
        public virtual IAssertion Assertion { get; } = StaticAssertion.False;

        internal ErrorFilter(IAssertion assertion, List<string> notList)
        {
            Assertion = assertion;
            _notifiers = notList;
        }

        public void OnErrorModuleFiltering(object sender, ExceptionFilterEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (args.Exception == null)
                throw new ArgumentException(null, nameof(args));

            try
            {
                if (Assertion.Test(new AssertionHelperContext(sender, args.Exception, args.Context)))
                {
                    if (Notifiers.Any())
                    {
                        args.DismissForNotifiers(Notifiers);
                    }
                    args.Dismiss();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                throw;
            }
        }

        private readonly List<string> _notifiers;

        public IEnumerable<string> Notifiers => _notifiers;

    }
}