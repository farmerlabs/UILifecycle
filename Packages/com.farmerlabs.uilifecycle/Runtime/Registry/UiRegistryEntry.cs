using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UiLifecycle
{
    /// <summary>
    /// 実体の供給手段。寿命 (LifetimePolicy) とは直交する軸。
    /// ※ 値は .asset に int で保存される。並べ替え / 途中挿入をしないこと (末尾に足す)。
    /// </summary>
    public enum ProviderKind
    {
        /// <summary>シーン配置済み (UiSceneAnchor が自己登録する)。SO はシーン参照を持てないため</summary>
        SceneObject,

        /// <summary>Prefab から Instantiate</summary>
        Prefab,

        /// <summary>シーンを Additive ロードし、その Root から IUiEntryPoint を得る</summary>
        AdditiveScene,

        /// <summary>コード登録した IUiInstanceProvider (Addressables 等の依存が要る調達は外部実装)</summary>
        Custom,
    }

    /// <summary>
    /// key → (供給手段, 寿命) の対応。呼び側に不可視。
    /// key は「UI の名前」= 名詞 (ID)。遷移名 (動詞) ではない
    /// (動詞 Show/Hide は API 側に既にある)。
    /// </summary>
    [Serializable]
    public sealed class UiRegistryEntry
    {
        [Header("共通")]
        [Tooltip("UI の名前 = 名詞 (ID)。呼び側が知る唯一の値")]
        [SerializeField] private string _key;
        [SerializeField] private LifetimePolicy _policy = LifetimePolicy.Transient;
        [SerializeField] private ProviderKind _kind = ProviderKind.Prefab;

        // 以下は Kind ごとの供給元。使うのは選んだ Kind の欄だけで、他は無視される。

        [Header("Kind = Prefab")]
        [Tooltip("Root に IUiEntryPoint 必須")]
        [SerializeField] private GameObject _prefab;

        [Header("Kind = AdditiveScene")]
#if UNITY_EDITOR
        [Tooltip("シーンアセット。ドロップすると下の Scene Name に反映される (Editor 専用フィールド)")]
        [SerializeField] private SceneAsset _scene;
#endif
        [Tooltip("実行時に使われるのはこちら。Scene から自動反映される。Root に IUiEntryPoint 必須。\n" +
                 "アタッチとは別に Build Settings の Scenes In Build への登録が必要 (Unity の制約)")]
        [SerializeField] private string _sceneName;

        [Header("Kind = Custom")]
        [Tooltip("UiHost.RegisterProvider の登録 ID")]
        [SerializeField] private string _customProviderId;

        public string Key => _key;
        public LifetimePolicy Policy => _policy;
        public ProviderKind Kind => _kind;
        public GameObject Prefab => _prefab;
        public string CustomProviderId => _customProviderId;
        public string SceneName => _sceneName;

#if UNITY_EDITOR
        /// <summary>
        /// アタッチされたシーンアセットの名前を実行時フィールドへ写す (UiRegistryAsset.OnValidate から)。
        ///
        /// SceneAsset は Editor 専用型でビルドに残せないため、実行時の真実は常に _sceneName。
        /// アタッチはリネーム追従と typo 防止のための入力補助であって、経路が 2 つになるわけではない。
        /// 未アタッチなら _sceneName には触らない — 手打ちも有効。
        /// </summary>
        internal void SyncSceneName()
        {
            if (_scene == null) return;
            _sceneName = _scene.name;
        }
#endif

#if UNITY_INCLUDE_TESTS
        /// <summary>テスト専用: コードから Entry を組む</summary>
        public static UiRegistryEntry CreateForTest(string key, LifetimePolicy policy, ProviderKind kind, GameObject prefab = null, string customProviderId = null, string sceneName = null)
        {
            return new UiRegistryEntry
            {
                _key = key,
                _policy = policy,
                _kind = kind,
                _prefab = prefab,
                _customProviderId = customProviderId,
                _sceneName = sceneName
            };
        }
#endif
    }
}
