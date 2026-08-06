using System;
using System.Collections.Generic;
using UnityEngine;

namespace UiLifecycle
{
    /// <summary>
    /// key の対応表 (SO)。呼び側は key しか知らないので、
    /// 寿命も供給手段もここで後から差し替えられる。
    /// 実行時の実体保持 (UiInstanceStore) も本アセットが持つ:
    /// シーン配置 UI (UiSceneAnchor) と UiHost が同じ SO を参照することで、
    /// static もシングルトンも DI も使わずに接点を作るため。
    /// </summary>
    [CreateAssetMenu(fileName = "UiRegistry", menuName = "UiLifecycle/Registry")]
    public sealed class UiRegistryAsset : ScriptableObject
    {
        [SerializeField] private List<UiRegistryEntry> _entries = new();

        [NonSerialized] private UiInstanceStore _store;
        [NonSerialized] private UiHost _host;
        [NonSerialized] private Dictionary<string, UiRegistryEntry> _index;

        public IReadOnlyList<UiRegistryEntry> Entries => _entries;

        /// <summary>実行時の実体保持。ドメインリロードで自然にリセットされる</summary>
        public UiInstanceStore Store => _store ??= new UiInstanceStore();

        /// <summary>
        /// 本 Registry に紐づく共有 Host。Store と同じ発想 (SO を接点にする) の遅延生成で、
        /// ドメインリロードで自然にリセットされる。
        /// UiShowButton 等の Inspector 配線コンポーネントはここから Host を得る
        /// (UiHost は純 C# なので SerializeField で参照できない)。
        /// 再入ゲート (sessions) は Host 単位のため、同一 Registry には 1 Host が原則。
        /// 自前の new UiHost(registry) も引き続き可能だが、共有 Host と併用しないこと。
        /// </summary>
        public UiHost Host => _host ??= new UiHost(this);

        public UiRegistryEntry Resolve(string key)
        {
            _index ??= BuildIndex();
            if (!_index.TryGetValue(key, out var entry))
                throw new KeyNotFoundException($"UiRegistry '{name}' に key '{key}' が無い。Registry に登録するか key を確認すること。");
            return entry;
        }

        private Dictionary<string, UiRegistryEntry> BuildIndex()
        {
            var index = new Dictionary<string, UiRegistryEntry>();
            foreach (var entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.Key)) continue;
                if (!index.TryAdd(entry.Key, entry))
                    Debug.LogError($"UiRegistry '{name}' : key '{entry.Key}' が重複している。先勝ちで解決する。", this);
            }
            return index;
        }

        private void OnValidate()
        {
            // インスペクタ編集を即反映
            _index = null;

#if UNITY_EDITOR
            // アタッチされたシーンアセット → 実行時フィールド (SceneAsset はビルドに残せないため)
            foreach (var entry in _entries)
                entry?.SyncSceneName();
#endif
        }

#if UNITY_INCLUDE_TESTS
        /// <summary>テスト専用: コードから Registry を組む</summary>
        public static UiRegistryAsset CreateForTest(params UiRegistryEntry[] entries)
        {
            var asset = CreateInstance<UiRegistryAsset>();
            asset._entries.AddRange(entries);
            return asset;
        }
#endif
    }
}
