using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UiLifecycle
{
    /// <summary>
    /// クリックで HideAsync(key) を呼ぶだけの使い回しボタン (親側 / 文脈フリー層)。
    /// UiShowButton と対になる: Registry と Key を Inspector で設定するだけ、コード 0 行。
    ///
    /// 親から閉じるので必ずキャンセル (HasValue=false) — 確定 (Close(result)) は
    /// ページ自身にしか作れない。ページの内側から閉じたい場合は UiCancelButton を使う。
    ///
    /// 対象が開いていなければ黙って no-op (UiHost.HideAsync が早期 return する)。
    /// UiShowButton の再入無視と同じく「押しても何も起きない」が正常系にある。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class UiHideButton : MonoBehaviour
    {
        [SerializeField] private UiRegistryAsset _registry;
        [SerializeField] private string _key;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (_registry == null || string.IsNullOrEmpty(_key))
            {
                Debug.LogError($"UiHideButton '{name}' : Registry / Key が未設定。", this);
                return;
            }
            _registry.Host.HideAsync(_key).Forget();
        }
    }
}
