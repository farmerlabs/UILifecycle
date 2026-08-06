using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UiLifecycle
{
    /// <summary>
    /// クリックで ShowAsync(key) を呼ぶだけの使い回しボタン (文脈フリー層)。
    /// Registry と Key を Inspector で設定するだけ、コード 0 行。
    /// Host は Registry の共有 Host を使う (UiHost は純 C# のため直接参照できない)。
    /// 型付き引数が要る呼び出しは対象外 — その時点でコードを書く層 (ShowAsync&lt;TArgs&gt;) に上がる。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class UiShowButton : MonoBehaviour
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
                Debug.LogError($"UiShowButton '{name}' : Registry / Key が未設定。", this);
                return;
            }
            _registry.Host.ShowAsync(_key).Forget();
        }
    }
}
