using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UiLifecycle.Samples.S006
{
    /// <summary>
    /// 006 — 演出その 1。CanvasGroup の alpha を動かすだけ。
    ///
    /// 基盤が要求するのは「UniTask で完了を返すこと」だけなので、
    /// 中身は Feel でも DOTween でも USS でもよい (ここでは依存を増やさず手書き)。
    /// EntryPoint と同じ GameObject に付けるだけで自動収集される。
    /// </summary>
    public sealed class FadePresenter : UiTransitionPresenterBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private float _duration = 0.25f;

        public override async UniTask PlayEnterAsync(CancellationToken ct)
        {
            // SetActive(true) の直後に呼ばれる。最初の 1 行で 0 にしておくと、
            // 描画される前に初期値が確定するので「一瞬だけ全部見える」が起きない。
            _group.alpha = 0f;
            for (var t = 0f; t < _duration; t += Time.deltaTime)
            {
                _group.alpha = t / _duration;
                await UniTask.Yield(ct);
            }
            _group.alpha = 1f;
        }

        public override async UniTask PlayExitAsync(CancellationToken ct)
        {
            for (var t = 0f; t < _duration; t += Time.deltaTime)
            {
                _group.alpha = 1f - t / _duration;
                await UniTask.Yield(ct);
            }
            _group.alpha = 0f;
        }
    }
}
