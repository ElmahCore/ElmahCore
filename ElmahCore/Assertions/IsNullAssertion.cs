namespace ElmahCore.Assertions
{
    internal sealed class IsNullAssertion : DataBoundAssertion
    {
        public IsNullAssertion(IContextExpression expression) :
            base(expression)
        {
        }

        protected override bool TestResult(object result)
        {
            return result == null;
        }
    }
}