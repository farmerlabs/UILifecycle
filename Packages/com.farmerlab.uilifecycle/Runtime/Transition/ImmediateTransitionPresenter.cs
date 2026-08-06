using System.Threading;
using Cysharp.Threading.Tasks;

namespace UiLifecycle
{
    /// <summary>
    /// 既定実装。即座に完了する (表示切替自体は EntryPoint 側の SetActive)。
    /// 演出在庫がゼロでも基盤が動くことを保証する。
    /// </summary>
    public sealed class ImmediateTransitionPresenter : IUiTransitionPresenter
    {
        public static readonly ImmediateTransitionPresenter Instance = new();

        private ImmediateTransitionPresenter() { }

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
