using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UiLifecycle
{
    /// <summary>
    /// 唯一の状態機械。順序を保証する:
    ///   調達 → Construct(args) → 入場演出待ち → Shown → (閉じ待ち) → 退場演出待ち → Hidden → ポリシーに従い解放
    ///
    /// ・引数注入は「生成時 1 回」ではなく「表示ごと」。
    /// ・退場演出の完了を await してから解放する (待たないとフェードアウト中に実体が消える)。
    /// ・再入 (同一 key の実行中に再度 ShowAsync) は「無視」— ただし await は必ず返す
    ///   (即座に HasValue=false)。ゲートはこの入口 1 箇所。無視/待つ/上書きは
    ///   戻り値契約が共通なので、後から方式を変えても呼び側は無変更。
    ///
    /// 純 C# クラス。公開の仕方 (DI / シングルトン / 直接 new) は使用者のアーキテクチャに委ねる。
    /// </summary>
    public sealed class UiHost : IUiHost
    {
        private readonly UiRegistryAsset _registry;
        private readonly Dictionary<string, IUiInstanceProvider> _customProviders = new();
        private readonly Dictionary<string, Session> _sessions = new();

        private sealed class Session
        {
            public IUiEntryPoint EntryPoint;

            /// <summary>
            /// 調達が完了する前に Hide が来た印。
            /// 調達が非同期な Provider (Additive Scene / Addressables) では
            /// EntryPoint がまだ無い窓が開くため、要求を取りこぼさずここに預ける。
            /// </summary>
            public bool CancelRequested;

            public readonly UniTaskCompletionSource Completion = new();
        }

        public UiHost(UiRegistryAsset registry)
        {
            _registry = registry != null ? registry : throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// Kind = Custom の供給手段を差す (Addressables / Additive Scene 等)。
        /// 基盤はこの差し込み口だけ持ち、実装依存を持たない。
        /// </summary>
        public void RegisterProvider(string id, IUiInstanceProvider provider)
        {
            _customProviders[id] = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public bool IsShown(string key)
        {
            return _sessions.TryGetValue(key, out var session)
                && session.EntryPoint != null
                && session.EntryPoint.Phase is UiPhase.Showing or UiPhase.Shown;
        }

        public async UniTask<UiResult<TResult>> ShowForResultAsync<TArgs, TResult>(string key, TArgs args, CancellationToken ct = default)
        {
            // 再入 = 無視 (awaitは即キャンセルで返る。永遠に返らない仕様にはしない)
            if (_sessions.ContainsKey(key)) return UiResult<TResult>.Canceled();

            var entry = _registry.Resolve(key);
            var provider = ResolveProvider(entry);
            var session = new Session();
            _sessions[key] = session;
            try
            {
                var untyped = await AcquireAsync(key, entry, provider, ct);
                if (untyped is not IUiEntryPoint<TArgs, TResult> typed)
                {
                    throw new InvalidOperationException(
                        $"key '{key}' の EntryPoint は {untyped.GetType().Name}。要求された UiEntryPoint<{typeof(TArgs).Name}, {typeof(TResult).Name}> と一致しない。");
                }
                session.EntryPoint = untyped;

                typed.Construct(args);              // ★ Start より前
                if (session.CancelRequested) typed.RequestCancel();   // 調達中に来ていた Hide を反映
                await typed.EnterAsync(ct);         // 入場演出の完了待ち
                var result = await typed.WaitForCloseAsync(ct);
                await CloseAsync(key, entry, provider, untyped, ct);
                return result;
            }
            finally
            {
                _sessions.Remove(key);
                session.Completion.TrySetResult();
            }
        }

        public UniTask<UiResult<TResult>> ShowForResultAsync<TResult>(string key, CancellationToken ct = default)
        {
            // 引数なし ShowAsync(key) と同じ手口。Unit は内部で詰まり、使用者は結果型だけ書く
            return ShowForResultAsync<Unit, TResult>(key, Unit.Default, ct);
        }

        public async UniTask ShowAsync<TArgs>(string key, TArgs args, CancellationToken ct = default)
        {
            if (_sessions.ContainsKey(key)) return;    // 再入 = 無視

            var entry = _registry.Resolve(key);
            var provider = ResolveProvider(entry);
            var session = new Session();
            _sessions[key] = session;
            var entered = false;
            try
            {
                var untyped = await AcquireAsync(key, entry, provider, ct);
                if (untyped is not IUiEntryPoint<TArgs> typed)
                {
                    throw new InvalidOperationException(
                        $"key '{key}' の EntryPoint は {untyped.GetType().Name}。要求された UiEntryPoint<{typeof(TArgs).Name}, TResult> と一致しない。");
                }
                session.EntryPoint = untyped;

                typed.Construct(args);
                if (session.CancelRequested) typed.RequestCancel();   // 調達中に来ていた Hide を反映
                await typed.EnterAsync(ct);          // Shown で呼び側の await が返る
                entered = true;

                // 閉じ〜解放は基盤が裏で面倒を見る (結果は誰も待っていない)
                RunCloseFlowAsync(key, entry, provider, untyped, session).Forget();
            }
            finally
            {
                if (!entered)
                {
                    _sessions.Remove(key);
                    session.Completion.TrySetResult();
                }
            }
        }

        public UniTask ShowAsync(string key, CancellationToken ct = default)
        {
            // 文脈フリー層の入口。Unit を詰めるだけで、挙動は上と同一
            return ShowAsync(key, Unit.Default, ct);
        }

        public async UniTask HideAsync(string key, CancellationToken ct = default)
        {
            if (!_sessions.TryGetValue(key, out var session)) return;

            // 開いた本人の ShowAsync はキャンセル (HasValue=false) で返る。
            // 調達中 (EntryPoint がまだ無い) なら印だけ残し、Construct 直後に効かせる
            // — 「入場演出中の Hide」と同じ挙動 (最後まで再生してから退場) に揃う。
            session.CancelRequested = true;
            session.EntryPoint?.RequestCancel();
            await session.Completion.Task.AttachExternalCancellation(ct);
        }

        // ─────────────────────────────────────────

        private async UniTaskVoid RunCloseFlowAsync(string key, UiRegistryEntry entry, IUiInstanceProvider provider, IUiEntryPoint entryPoint, Session session)
        {
            try
            {
                await entryPoint.WaitForCloseRequestAsync(CancellationToken.None);
                await CloseAsync(key, entry, provider, entryPoint, CancellationToken.None);
            }
            finally
            {
                _sessions.Remove(key);
                session.Completion.TrySetResult();
            }
        }

        private async UniTask CloseAsync(string key, UiRegistryEntry entry, IUiInstanceProvider provider, IUiEntryPoint entryPoint, CancellationToken ct)
        {
            await entryPoint.ExitAsync(ct);          // 退場演出の完了待ち。これより前に解放しない

            if (entry.Policy == LifetimePolicy.Transient)
            {
                _registry.Store.Remove(key);
                await provider.ReleaseAsync(entryPoint, ct);
            }
        }

        private async UniTask<IUiEntryPoint> AcquireAsync(string key, UiRegistryEntry entry, IUiInstanceProvider provider, CancellationToken ct)
        {
            var store = _registry.Store;

            // Persistent / Cached は key ごと 1 インスタンスを再利用
            if (entry.Policy != LifetimePolicy.Transient && store.TryGet(key, out var existing))
                return existing;

            if (entry.Kind == ProviderKind.SceneObject)
            {
                throw new InvalidOperationException(
                    $"key '{key}' は SceneObject だが、シーンに UiSceneAnchor(key='{key}') が登録されていない。" +
                    "アンカーの Registry 参照と key、GameObject がアクティブか確認すること。");
            }

            var entryPoint = await provider.AcquireAsync(ct);
            if (entry.Policy == LifetimePolicy.Cached)
                store.Set(key, entryPoint);
            return entryPoint;
        }

        private IUiInstanceProvider ResolveProvider(UiRegistryEntry entry)
        {
            switch (entry.Kind)
            {
                case ProviderKind.SceneObject:
                    if (entry.Policy != LifetimePolicy.Persistent)
                        throw new InvalidOperationException(
                            $"key '{entry.Key}' : SceneObject は Persistent 限定 (シーン配置 UI を基盤が生成/破棄することはできない)。");
                    return null; // 調達は UiSceneAnchor の自己登録 (Store) 経由

                case ProviderKind.Prefab:
                    RequireAcquirablePolicy(entry, "Prefab");
                    if (entry.Prefab == null)
                        throw new InvalidOperationException($"key '{entry.Key}' : Prefab 未設定。");
                    return new PrefabProvider(entry.Prefab);

                case ProviderKind.AdditiveScene:
                    RequireAcquirablePolicy(entry, "AdditiveScene");
                    if (string.IsNullOrEmpty(entry.SceneName))
                        throw new InvalidOperationException($"key '{entry.Key}' : SceneName 未設定。");
                    return new AdditiveSceneProvider(entry.SceneName);

                case ProviderKind.Custom:
                    RequireAcquirablePolicy(entry, "Custom");
                    if (string.IsNullOrEmpty(entry.CustomProviderId) || !_customProviders.TryGetValue(entry.CustomProviderId, out var custom))
                        throw new InvalidOperationException(
                            $"key '{entry.Key}' : CustomProviderId '{entry.CustomProviderId}' が未登録。UiHost.RegisterProvider で先に登録すること。");
                    return custom;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entry.Kind));
            }
        }

        /// <summary>
        /// Persistent は「シーンに配置済み = 基盤は調達しない」の意味なので、
        /// 調達する Provider と組み合わせられない (指定が自己矛盾になる)。
        /// 塞いでも表現力は落ちない: 常駐させたいなら SceneObject + Persistent、
        /// 遅延常駐なら Cached が同じ挙動を覆う。
        /// </summary>
        private static void RequireAcquirablePolicy(UiRegistryEntry entry, string kindName)
        {
            if (entry.Policy != LifetimePolicy.Persistent) return;

            throw new InvalidOperationException(
                $"key '{entry.Key}' : {kindName} は Cached / Transient 限定 (Persistent は基盤が調達しない SceneObject 専用)。" +
                "シーン常駐は SceneObject + Persistent、遅延常駐は Cached を使う。");
        }
    }
}
