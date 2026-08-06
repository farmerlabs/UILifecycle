using System;

namespace UiLifecycle
{
    /// <summary>
    /// IOnceEvent の最小実装。
    /// 表示ごとに Construct が呼ばれるため、厳密には「最新値を記憶し
    /// 購読時に即再生する」イベント (BehaviorSubject 相当) として振る舞う。
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
