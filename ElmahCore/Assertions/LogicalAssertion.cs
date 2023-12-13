using System;
using System.Collections.Generic;

namespace ElmahCore.Assertions
{
    internal sealed class LogicalAssertion : CompositeAssertion
    {
        private readonly bool _all;
        private readonly bool _not;

        private LogicalAssertion(IEnumerable<IAssertion> assertions, bool not, bool all) :
            base(assertions)
        {
            _not = not;
            _all = all;
        }

        public static LogicalAssertion LogicalAnd(IAssertion[] operands)
        {
            return new LogicalAssertion(operands, false, true);
        }

        public static LogicalAssertion LogicalOr(IAssertion[] operands)
        {
            return new LogicalAssertion(operands, false, false);
        }

        public static LogicalAssertion LogicalNot(IAssertion[] operands)
        {
            return new LogicalAssertion(operands, true, true);
        }

        public override bool Test(AssertionHelperContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (Count == 0)
                return false;

            //
            // Walk through all child assertions and determine the
            // outcome, OR-ing or AND-ing each as needed.
            //

            var result = false;

            foreach (var assertion in this)
            {
                if (assertion == null)
                    continue;

                var testResult = assertion.Test(context);

                if (_not)
                    testResult = !testResult;

                if (testResult)
                {
                    if (!_all) return true;
                    result = true;
                }
                else
                {
                    if (_all) return false;
                }
            }

            return result;
        }
    }
}