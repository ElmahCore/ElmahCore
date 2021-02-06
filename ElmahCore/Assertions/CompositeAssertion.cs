using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ElmahCore.Assertions
{
    /// <summary>
    ///     Read-only collection of <see cref="Assertions.IAssertion" /> instances.
    /// </summary>
    [Serializable]
    internal abstract class CompositeAssertion : ReadOnlyCollection<IAssertion>, IAssertion
    {
        protected CompositeAssertion() :
            this(Enumerable.Empty<IAssertion>())
        {
        }

        protected CompositeAssertion(IEnumerable<IAssertion> assertions) :
            base(Validate(assertions).ToArray())
        {
        }

        public abstract bool Test(object context);

        private static IEnumerable<IAssertion> Validate(IEnumerable<IAssertion> assertions)
        {
            if (assertions == null) throw new ArgumentNullException(nameof(assertions));
            return ValidateImpl(assertions);
        }

        private static IEnumerable<IAssertion> ValidateImpl(IEnumerable<IAssertion> assertions)
        {
            foreach (var assertion in assertions)
            {
                if (assertion == null)
                    throw new ArgumentException(null, nameof(assertions));
                yield return assertion;
            }
        }
    }
}