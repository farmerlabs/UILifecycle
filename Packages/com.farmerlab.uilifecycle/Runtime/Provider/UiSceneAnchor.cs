using UnityEngine;

namespace UiLifecycle
{
    /// <summary>
    /// シーン配置 UI (Persistent) の自己登録。
    /// Registry は SO であり、SO はシーンのインスタンスを参照できない。
    /// そのためシーン側から Registry の Store へ登録する向きにする
    /// (SO を長命な中継点にする — Channel と同じ構図)。
    ///
    /// EntryPoint と同じ GameObject にアタッチする。
    /// 注意: 非アクティブな GameObject では Awake が走らず登録されない。
    /// 初期非表示にしたい場合はアクティブのまま _hideOnAwake を使う。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UiSceneAnchor : MonoBehaviour
    {
        [SerializeField] private UiRegistryAsset _registry;
        [SerializeField] private string _key;

        [Tooltip("登録後に非表示化する (初期非表示のシーン配置 UI 用)")]
        [SerializeField] private bool _hideOnAwake = true;

        private void Awake()
        {
            if (_registry == null)
            {
                Debug.LogError($"UiSceneAnchor '{name}' : Registry 未設定。", this);
                return;
            }
            if (!TryGetComponent<IUiEntryPoint>(out var entryPoint))
            {
                Debug.LogError($"UiSceneAnchor '{name}' : 同じ GameObject に IUiEntryPoint が無い。", this);
                return;
            }

            _registry.Store.Set(_key, entryPoint);

            if (_hideOnAwake) gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_registry == null) return;
            if (_registry.Store.TryGet(_key, out var current) && ReferenceEquals(current.Root, gameObject))
                _registry.Store.Remove(_key);
        }
    }
}
