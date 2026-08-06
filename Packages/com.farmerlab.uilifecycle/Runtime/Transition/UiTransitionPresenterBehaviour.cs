using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UiLifecycle
{
    /// <summary>
    /// コンポーネントとして演出を差すための基底。
    /// EntryPoint と同じ GameObject にアタッチすると自動収集される
    /// (複数あれば CompositeTransitionPresenter で束ねられる)。
    /// プロダクト側の Feel ラッパー等はこれを継承する。
    /// </summary>
    public abstract class UiTransitionPresenterBehaviour : MonoBehaviour, IUiTransitionPresenter
    {
        public abstract UniTask PlayEnterAsync(CancellationToken ct);
        public abstract UniTask PlayExitAsync(CancellationToken ct);
    }
}
