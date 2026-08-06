using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UiLifecycle
{
    /// <summary>
    /// 使用者が継承する唯一のクラス。書くのは OnShow と、閉じたい時の Close/Cancel だけ。
    ///
    /// ・OnShow(args) は「表示するたび」に呼ばれる。
    ///   → 再表示 = 初期状態 が既定の作法になる。
    ///     Persistent/Cached で状態が残る問題を「毎回 args から組み立て直す」に寄せて潰す。
    /// ・Close(result) を呼ぶと ShowAsync の await が返る。
    ///   呼び側とページの間にイベント購読もコールバック登録も要らない。
    /// ・演出は同じ GameObject の IUiTransitionPresenter コンポーネントを自動収集する。
    ///   0 個 → Immediate (即完了)、複数 → Composite(Parallel) で合成。
    /// </summary>
    public abstract class UiEntryPoint<TArgs, TResult> : MonoBehaviour, IUiEntryPoint<TArgs, TResult>
    {
        [Header("演出合成 (Presenter が複数ある場合)")]
        [SerializeField] private ExecutionMode _presenterMode = ExecutionMode.Parallel;

        private readonly OnceEvent<TArgs> _onConstructed = new();
        private UniTaskCompletionSource<UiResult<TResult>> _closeSource;
        private IUiTransitionPresenter _presenter;

        public UiPhase Phase { get; private set; } = UiPhase.Hidden;
        public GameObject Root => gameObject;

        /// <summary>
        /// 表示引数の到着通知。発火済みなら購読時に即再生されるため、
        /// 動的生成 (Instantiate 直後に Construct) でも取りこぼさない。
        /// </summary>
        public IOnceEvent<TArgs> OnConstructed => _onConstructed;

        /// <summary>
        /// 表示引数の注入。基盤が Start より前に呼ぶことを保証する
        /// (使用者は「OnShow で受け取り、Start 以降は入っている」とだけ覚える)。
        /// 静的 UI へは使用者側の任意経路 (ViewModel / View→View) からも呼べる。
        /// </summary>
        public void Construct(TArgs args)
        {
            _closeSource = new UniTaskCompletionSource<UiResult<TResult>>();
            OnShow(args);
            _onConstructed.Emit(args);
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            Phase = UiPhase.Showing;
            gameObject.SetActive(true);
            await Presenter.PlayEnterAsync(ct);
            Phase = UiPhase.Shown;
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            Phase = UiPhase.Hiding;
            await Presenter.PlayExitAsync(ct);
            gameObject.SetActive(false);
            Phase = UiPhase.Hidden;
        }

        public UniTask<UiResult<TResult>> WaitForCloseAsync(CancellationToken ct)
        {
            return _closeSource.Task.AttachExternalCancellation(ct);
        }

        UniTask IUiEntryPoint.WaitForCloseRequestAsync(CancellationToken ct)
        {
            return WaitForCloseAsync(ct);
        }

        public void RequestCancel()
        {
            _closeSource?.TrySetResult(UiResult<TResult>.Canceled());
        }

        /// <summary>
        /// 表示引数から画面を組み立てる。表示のたびに呼ばれる。
        /// ここで毎回組み立て直すことが「再表示 = 初期状態」の実体。
        /// </summary>
        protected abstract void OnShow(TArgs args);

        /// <summary>
        /// 確定して閉じる (HasValue=true)。子だけが呼べる —
        /// HasValue=true は「ページが Close(result) を呼んだ」と一意に対応する。
        /// </summary>
        protected void Close(TResult result)
        {
            _closeSource?.TrySetResult(UiResult<TResult>.Of(result));
        }

        /// <summary>取り消して閉じる (×ボタン / 戻る。HasValue=false)</summary>
        protected void Cancel()
        {
            _closeSource?.TrySetResult(UiResult<TResult>.Canceled());
        }

        private IUiTransitionPresenter Presenter
        {
            get
            {
                if (_presenter != null) return _presenter;

                var found = new List<IUiTransitionPresenter>();
                GetComponents(found);
                _presenter = found.Count switch
                {
                    0 => ImmediateTransitionPresenter.Instance,
                    1 => found[0],
                    _ => new CompositeTransitionPresenter(found, _presenterMode)
                };
                return _presenter;
            }
        }
    }
}
