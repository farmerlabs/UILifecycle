using System;

namespace UiLifecycle
{
    /// <summary>
    /// IOnceEvent の最小実装 (ReplaySubject(1) 相当)。
    /// 最新値を保持し、購読時に発火済みなら即再生する。
    /// </summary>
    public sealed class OnceEvent<T> : IOnceEvent<T>
    {
        private Action<T> _handlers;
        private bool _hasValue;
        private T _last;

        public IDisposable Subscribe(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _handlers += handler;
            if (_hasValue) handler(_last);
            return new Subscription(this, handler);
        }

        internal void Emit(T value)
        {
            _hasValue = true;
            _last = value;
            _handlers?.Invoke(value);
        }

        private sealed class Subscription : IDisposable
        {
            private OnceEvent<T> _owner;
            private Action<T> _handler;

            public Subscription(OnceEvent<T> owner, Action<T> handler)
            {
                _owner = owner;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_owner == null) return;
                _owner._handlers -= _handler;
                _owner = null;
                _handler = null;
            }
        }
    }
}
