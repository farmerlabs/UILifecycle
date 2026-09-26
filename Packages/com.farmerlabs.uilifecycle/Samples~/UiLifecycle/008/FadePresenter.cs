using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UiLifecycle.Samples.S008
{
    /// <summary>
    /// CanvasGroup の alpha を動かすだけの演出 (006 と同じもの)。
    /// 演出は供給手段と直交するので、Custom で調達しても扱いは変わらない。
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
