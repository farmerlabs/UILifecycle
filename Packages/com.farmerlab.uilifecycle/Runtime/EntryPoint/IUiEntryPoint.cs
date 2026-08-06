using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UiLifecycle
{
    /// <summary>
    /// UI の Root にアタッチする規約。
    /// Root にあると決めることで、Additive Scene でも
    /// scene.GetRootGameObjects() から 1 回で発見でき、Prefab 運用と同じ形で扱える。
    /// 非ジェネリック側は UiHost が型を知らずに操作するための最小契約。
    /// </summary>
    public interface IUiEntryPoint
    {
        UiPhase Phase { get; }
        GameObject Root { get; }

        /// <summary>入場。演出の完了まで返らない</summary>
        UniTask EnterAsync(CancellationToken ct);

        /// <summary>退場。演出の完了まで返らない (これを待ってから解放する)</summary>
        UniTask ExitAsync(CancellationToken ct);

        /// <summary>閉じ要求 (Close / Cancel / RequestCancel) が来るまで待つ</summary>
        UniTask WaitForCloseRequestAsync(CancellationToken ct);

        /// <summary>
        /// ページの外からのキャンセル要求 (HideAsync / UiCancelButton)。
        /// 外から作れるのはキャンセルだけ — 確定 (Close(result)) は
        /// ページが自分の状態から組み立てるものなので、ここには対になる API を置かない。
        /// </summary>
        void RequestCancel();
    }

    /// <summary>表示引数 (in) を受け取れる EntryPoint</summary>
    public interface IUiEntryPoint<in TArgs> : IUiEntryPoint
    {
        /// <summary>
        /// 表示引数の注入。「生成時 1 回」ではなく「表示ごと」に毎回呼ばれる。
        /// 基盤は Start より前に呼ばれることを保証する。
        /// </summary>
        void Construct(TArgs args);
    }

    /// <summary>結果 (out) も返せる EntryPoint</summary>
    public interface IUiEntryPoint<in TArgs, TResult> : IUiEntryPoint<TArgs>
    {
        /// <summary>閉じ要求を待ち、結果 (値 or キャンセル) を受け取る</summary>
        UniTask<UiResult<TResult>> WaitForCloseAsync(CancellationToken ct);
    }
}
