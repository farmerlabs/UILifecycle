using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UiLifecycle.Tests
{
    /// <summary>
    /// 調達を任意のタイミングまで止められる Provider。
    /// Additive Scene / Addressables の「調達がフレームを跨ぐ」状況を、
    /// シーンもアセットも用意せずに再現するためのテスト用実装。
    /// </summary>
    public sealed class FakeSlowProvider : IUiInstanceProvider
    {
        private readonly GameObject _template;
        private readonly UniTaskCompletionSource _gate = new();

        public bool IsAcquiring { get; private set; }
        public int ReleaseCount { get; private set; }

        public FakeSlowProvider(GameObject template) => _template = template;

        /// <summary>止めていた調達を完了させる</summary>
        public void CompleteAcquire() => _gate.TrySetResult();

        public async UniTask<IUiEntryPoint> AcquireAsync(CancellationToken ct)
        {
            IsAcquiring = true;
            await _gate.Task;
            IsAcquiring = false;

            // template は非アクティブなので複製も非アクティブ
            // = IUiInstanceProvider の「調達直後は非表示」契約を満たしている
            var instance = Object.Instantiate(_template);
            return instance.GetComponent<IUiEntryPoint>();
        }

        public UniTask ReleaseAsync(IUiEntryPoint entryPoint, CancellationToken ct)
        {
            ReleaseCount++;
            if (entryPoint?.Root != null) Object.Destroy(entryPoint.Root);
            return UniTask.CompletedTask;
        }
    }
}
