using System;

namespace UiLifecycle
{
    /// <summary>
    /// 「直近の発火を記憶する」イベント。
    /// 動的生成では Instantiate 直後に Construct が走るため、
    /// 素の event Action では外部が購読する隙が無く必ず取りこぼす。
    /// 後から購読しても、発火済みなら即座にコールバックが返る。
    /// (UniRx の ReplaySubject(1) 相当。UniRx を必須にしないため自前で最小実装)
    ///
    /// Once は「1 回しか発火しない」ではなく「取りこぼさない」の意。
    /// 使い回す実体では表示ごとに Construct = 再発火する。
    /// </summary>
    public interface IOnceEvent<T>
    {
        IDisposable Subscribe(Action<T> handler);
    }
}
