using System;

namespace ElmahCore.Assertions
{
	internal sealed class DelegatedContextExpression : IContextExpression
    {
        public DelegatedContextExpression(Func<object, object> handler)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public Func<object, object> Handler { get; }

        public object Evaluate(object context) => Handler(context);

        public override string ToString() => Handler.ToString();
    }
}