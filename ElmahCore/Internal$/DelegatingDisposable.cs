using System;

namespace ElmahCore
{
    sealed class DelegatingDisposable : IDisposable
    {
        private Action _disposer;

        public DelegatingDisposable(Action disposer)
        {
	        _disposer = disposer ?? throw new ArgumentNullException(nameof(disposer));
        }

        public void Dispose()
        {
            var disposer = _disposer;
            if (disposer == null)
                return;
            _disposer = null;
            disposer();
        }
    }
}