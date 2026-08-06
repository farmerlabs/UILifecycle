using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UiLifecycle.Samples.S004
{
    /// <summary>
    /// 004 — 待機あり / 引数あり / 返り値あり。全部入り。
    ///
    /// 003 に引数が乗っただけで、await の性質は同じ。
    /// おまけに「第三者から閉じる」(HideAsync) も置いてある —
    /// 開いた本人以外が畳む経路で、本人の await は HasValue=false で返る。
    /// </summary>
    public sealed class Sample004Bootstrap : MonoBehaviour
    {
        [SerializeField] private UiRegistryAsset _registry;
        [SerializeField] private string _key = "ItemDetail";
        [SerializeField] private int _itemId = 100;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _hideButton;
        [SerializeField] private TMP_Text _resultText;

        private void Start()
        {
            _openButton.onClick.AddListener(OnOpenClicked);
            _hideButton.onClick.AddListener(HideDetail);
        }

        private void OnDestroy()
        {
            // 自分が張った分だけ剥がす (RemoveAllListeners は同じ Button 上の
            // 他コンポーネントの購読を踏み潰すため使わない)
            _openButton.onClick.RemoveListener(OnOpenClicked);
            _hideButton.onClick.RemoveListener(HideDetail);
        }

        private void OnOpenClicked() => OpenDetailAsync(_itemId).Forget();

        private async UniTaskVoid OpenDetailAsync(int itemId)
        {
            var result = await _registry.Host.ShowForResultAsync<ItemDetailArgs, string>(
                _key, new ItemDetailArgs(itemId));

            _resultText.text = result.HasValue ? $"Result: {result.Value}" : "(canceled)";
        }

        /// <summary>
        /// 第三者が閉じる経路 (画面遷移で全部畳む等)。
        /// 退場演出〜解放の完了まで待てるが、ここでは待たずに投げっぱなしにしている。
        /// </summary>
        private void HideDetail() => _registry.Host.HideAsync(_key).Forget();
    }
}
