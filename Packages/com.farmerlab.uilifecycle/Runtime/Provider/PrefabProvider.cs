using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UiLifecycle
{
    /// <summary>
    /// Prefab から Instantiate する供給手段。
    /// Acquire は同期完了する (await しても同フレーム) — このため
    /// Instantiate → Construct が同フレームに収まり、Start 前注入の保証が成立する。
    /// </summary>
    public sealed class PrefabProvider : IUiInstanceProvider
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;

        public PrefabProvider(GameObject prefab, Transform parent = null)
        {
            _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            _parent = parent;
        }

        public UniTask<IUiEntryPoint> AcquireAsync(CancellationToken ct)
        {
            var instance = _parent != null
                ? UnityEngine.Object.Instantiate(_prefab, _parent, false)
                : UnityEngine.Object.Instantiate(_prefab);

            if (!instance.TryGetComponent<IUiEntryPoint>(out var entryPoint))
            {
                UnityEngine.Object.Destroy(instance);
                throw new InvalidOperationException(
                    $"Prefab '{_prefab.name}' の Root に IUiEntryPoint が無い。UiEntryPoint<TArgs,TResult> を継承したコンポーネントを Root にアタッチすること。");
            }

            return UniTask.FromResult(entryPoint);
        }

        public UniTask ReleaseAsync(IUiEntryPoint entryPoint, CancellationToken ct)
        {
            // 破棄済みの実体に .Root (= gameObject) を触ると MissingReferenceException。
            // ?. は素の参照比較なので、ここを通り抜けてしまう
            if (!UiObject.IsAlive(entryPoint)) return UniTask.CompletedTask;

            if (entryPoint.Root != null)
                UnityEngine.Object.Destroy(entryPoint.Root);
            return UniTask.CompletedTask;
        }
    }
}
