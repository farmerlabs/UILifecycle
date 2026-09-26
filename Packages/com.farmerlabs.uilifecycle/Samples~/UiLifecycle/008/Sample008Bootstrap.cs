using UnityEngine;

namespace UiLifecycle.Samples.S008
{
    /// <summary>
    /// 008 — Custom Provider で調達を差す。
    ///
    /// 生成先 (parent) はシーン内の Transform なので、アセットである Registry (SO) は
    /// 持てない。よって「Registry に生成先の欄を足す」ではなく、
    /// シーン側から Provider ごと差し込む。
    ///
    /// PrefabProvider はパッケージ標準の実装をそのまま使う —
    /// Custom は Addressables のような外部依存が要る調達の専用口ではない。
    /// </summary>
    public sealed class Sample008Bootstrap : MonoBehaviour
    {
        [SerializeField] private UiRegistryAsset _registry;
        [SerializeField] private GameObject _pagePrefab;
        [SerializeField] private Transform _leftAnchor;
        [SerializeField] private Transform _rightAnchor;

        private void Awake()
        {
            // Registry の CustomProviderId と同じ文字列で登録する。
            // Host は再生のたびに作り直されるので、登録も毎回必要。
            var host = _registry.Host;
            host.RegisterProvider("Anchor_Left", new PrefabProvider(_pagePrefab, _leftAnchor));
            host.RegisterProvider("Anchor_Right", new PrefabProvider(_pagePrefab, _rightAnchor));
        }
    }
}
