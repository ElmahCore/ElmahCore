using System;

namespace ElmahCore.Assertions
{
    internal abstract class DataBoundAssertion : IAssertion
    {
        protected DataBoundAssertion(IContextExpression expression) => Expression = expression ?? throw new ArgumentNullException(nameof(expression));

        protected IContextExpression Expression { get; }

        public virtual bool Test(object context) =>
            context == null
                ? throw new ArgumentNullException(nameof(context))
                : TestResult(Expression.Evaluate(context));

        protected abstract bool TestResult(object result);
    }
}