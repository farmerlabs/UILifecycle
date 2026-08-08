using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace UiLifecycle
{
    /// <summary>
    /// シーンを Additive ロードし、その Root から IUiEntryPoint を得る供給手段。
    /// SceneManager は Unity 標準 (CoreModule) なので依存は増えない — 基盤に置く判断の根拠は
    /// 「依存が乗るか」の一点であり、Addressables を外に出したのと同じ基準による。
    ///
    /// ・解放対象は entryPoint.Root.scene から復元できるため、本 Provider は状態を持たない。
    /// ・Acquire はフレームを跨ぐ。IUiInstanceProvider の「調達直後は非表示」契約を
    ///   満たすため、sceneLoaded (= Awake/OnEnable の直後・Start の前) で Root を落とす。
    ///   これで Prefab と同じ「Awake → Construct → 入場 → Start」の順が成立する。
    ///
    /// 運用上の制約 (Prefab には無いもの):
    ///   ・UI シーンに EventSystem / Camera を置かない (重複で入力が壊れる)
    ///   ・Unload はシーン所属の GameObject しか消さない。UI シーンから他シーンへ生成物を漏らさない
    ///   ・1 シーン 1 ページ。同じシーン名を複数の key に割り当てない
    /// </summary>
    public sealed class AdditiveSceneProvider : IUiInstanceProvider
    {
        private readonly string _sceneName;

        public AdditiveSceneProvider(string sceneName)
        {
            _sceneName = !string.IsNullOrEmpty(sceneName)
                ? sceneName
                : throw new ArgumentException("シーン名が空。", nameof(sceneName));
        }

        public async UniTask<IUiEntryPoint> AcquireAsync(CancellationToken ct)
        {
            Scene loaded = default;
            IUiEntryPoint found = null;

            // sceneLoaded はロード対象の Awake/OnEnable の直後、Start の前に同期で来る。
            // ここで拾えば「どのシーンがロードされたか」が確定し (sceneCount-1 の推測が要らない)、
            // Root を落とせば Start が Construct を追い越さず、素の状態も 1 フレームも見えない。
            void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (mode != LoadSceneMode.Additive || scene.name != _sceneName) return;
                loaded = scene;

                foreach (var root in scene.GetRootGameObjects())
                {
                    if (!root.TryGetComponent<IUiEntryPoint>(out var entryPoint)) continue;
                    found = entryPoint;
                    root.SetActive(false);
                    break;
                }
            }

            var operation = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                throw new InvalidOperationException(
                    $"シーン '{_sceneName}' をロードできない。Build Settings の Scenes In Build に追加されているか、シーン名が正しいか確認すること。");
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            try
            {
                // ct はロードに渡さない。途中で切ると半ロードのシーンの後始末と ShowAsync の
                // 戻り契約が増えるため。調達中の Hide は UiHost が預かり、Construct 直後に効く。
                await operation.ToUniTask();
            }
            finally
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }

            if (found != null) return found;

            if (loaded.IsValid()) await SceneManager.UnloadSceneAsync(loaded).ToUniTask();
            throw new InvalidOperationException(
                $"シーン '{_sceneName}' の Root GameObject に IUiEntryPoint が無い。" +
                "UiEntryPoint<TArgs,TResult> を継承したコンポーネント (UiPage 等) を Root にアタッチすること " +
                "(Canvas の下ではなく Root 直下。Prefab と同じ規則)。");
        }

        public UniTask ReleaseAsync(IUiEntryPoint entryPoint, CancellationToken ct)
        {
            // 破棄済みの実体に .Root を触ると MissingReferenceException (?. では検出できない)
            if (!UiObject.IsAlive(entryPoint) || entryPoint.Root == null) return UniTask.CompletedTask;

            var scene = entryPoint.Root.scene;
            if (!scene.IsValid() || !scene.isLoaded) return UniTask.CompletedTask;

            return SceneManager.UnloadSceneAsync(scene).ToUniTask();
        }
    }
}
