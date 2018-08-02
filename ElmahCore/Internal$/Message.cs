using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ElmahCore
{
    #region Imports

    #endregion

    internal class Message<TInput, TOutput>
    {
        sealed class State
        {
            public Func<Func<object, TInput, TOutput>, Func<object, TInput, TOutput>> Binder { get; }
            public Func<object, TInput, TOutput> Handler { get; }

            public State(Func<Func<object, TInput, TOutput>, Func<object, TInput, TOutput>> binder, Func<object, TInput, TOutput> handler)
            {
                Binder = binder;
                Handler = handler;
            }
        }

        private State _state = new State(null, null);

        bool TryUpdateState(State replacement, State current)
        {
            return current == Interlocked.CompareExchange(ref _state, replacement, current);
        }

        public IDisposable PushHandler(Func<Func<object, TInput, TOutput>, Func<object, TInput, TOutput>> binder)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));

            for (var updated = false; !updated; )
            {
                var current = _state;
                var state = new State(current.Binder + binder, null);
                updated = TryUpdateState(state, current);
            }
            return new DelegatingDisposable(() => RemoveHandler(binder));
        }

        void RemoveHandler(Func<Func<object, TInput, TOutput>, Func<object, TInput, TOutput>> binder)
        {
            Debug.Assert(binder != null);

            for (var updated = false; !updated; )
            {
                var current = _state;
	            // ReSharper disable once DelegateSubtraction
	            var state = new State(current.Binder - binder, null);
                updated = TryUpdateState(state, current);
            }
        }

        public TOutput Send(TInput input)
        {
            return Send(null, input);
        }

        public virtual TOutput Send(object sender, TInput input)
        {
            Func<object, TInput, TOutput> handler = null;
            for (var updated = false; !updated; )
            {
                var state = _state;
                handler = state.Handler;
                if (handler != null)
                    break;
                var binder = _state.Binder;
                if (binder == null)
                    return default(TOutput);
                var delegates = binder.GetInvocationList();
                var binders =
                    from Func<Func<object, TInput, TOutput>, Func<object, TInput, TOutput>> d
                        in delegates
                    select d;
                handler = binders.Aggregate((Func<object, TInput, TOutput>) delegate { return default(TOutput); }, (next, b) => b(next));
                updated = TryUpdateState(new State(state.Binder, handler), state);
            }

            return handler(sender, input);
        }
    }
}