using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UiLifecycle
{
    /// <summary>
    /// 1 key : 1 EntryPoint : n 演出対象 の「n」を束ねる場所。
    /// 複数 Canvas / 複数部位を持つ UI は EntryPoint がこれで内側に合成する。
    /// n は Registry に漏らさない (key は 1:1 のまま)。
    /// 合成しても IUiTransitionPresenter なので、UiHost からは単体演出と区別がつかない。
    /// </summary>
    public sealed class CompositeTransitionPresenter : IUiTransitionPresenter
    {
        private readonly IReadOnlyList<IUiTransitionPresenter> _children;
        private readonly ExecutionMode _mode;

        public CompositeTransitionPresenter(IReadOnlyList<IUiTransitionPresenter> children, ExecutionMode mode = ExecutionMode.Parallel)
        {
            _children = children;
            _mode = mode;
        }

        public async UniTask PlayEnterAsync(CancellationToken ct)
        {
            if (_mode == ExecutionMode.Parallel)
            {
                await UniTask.WhenAll(_children.Select(c => c.PlayEnterAsync(ct)));
            }
            else
            {
                foreach (var child in _children)
                    await child.PlayEnterAsync(ct);
            }
        }

        public async UniTask PlayExitAsync(CancellationToken ct)
        {
            if (_mode == ExecutionMode.Parallel)
            {
                await UniTask.WhenAll(_children.Select(c => c.PlayExitAsync(ct)));
            }
            else
            {
                foreach (var child in _children)
                    await child.PlayExitAsync(ct);
            }
        }
    }
}
