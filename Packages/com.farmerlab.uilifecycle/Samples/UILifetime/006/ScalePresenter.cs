using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UiLifecycle.Samples.S006
{
    /// <summary>
    /// 006 — 演出その 2。localScale を動かすだけ。
    ///
    /// FadePresenter と「同じ GameObject に 2 つ付いている」状態を作るためだけに存在する。
    /// 2 つ以上あると EntryPoint が CompositeTransitionPresenter で束ね、
    /// インスペクタの Presenter Mode (Parallel / Sequential) が効くようになる。
    /// </summary>
    public sealed class ScalePresenter : UiTransitionPresenterBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private float _duration = 0.25f;
        [SerializeField] private float _from = 0.8f;

        public override async UniTask PlayEnterAsync(CancellationToken ct)
        {
            _target.localScale = Vector3.one * _from;
            for (var t = 0f; t < _duration; t += Time.deltaTime)
            {
                _target.localScale = Vector3.one * Mathf.Lerp(_from, 1f, t / _duration);
                await UniTask.Yield(ct);
            }
            _target.localScale = Vector3.one;
        }

        public override async UniTask PlayExitAsync(CancellationToken ct)
        {
            for (var t = 0f; t < _duration; t += Time.deltaTime)
            {
                _target.localScale = Vector3.one * Mathf.Lerp(1f, _from, t / _duration);
                await UniTask.Yield(ct);
            }
            _target.localScale = Vector3.one * _from;
        }
    }
}
