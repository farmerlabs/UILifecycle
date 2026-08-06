using System;

namespace UiLifecycle
{
    /// <summary>
    /// 「発火済みを記憶する」イベント。
    /// 動的生成では Instantiate 直後に Construct が走るため、
    /// 素の event Action では外部が購読する隙が無く必ず取りこぼす。
    /// 後から購読しても、発火済みなら即座にコールバックが返る。
    /// (UniRx の AsyncSubject 相当。UniRx を必須にしないため自前で最小実装)
    /// </summary>
    public interface IOnceEvent<T>
    {
        IDisposable Subscribe(Action<T> handler);
    }
}
