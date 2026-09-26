using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UiLifecycle.Samples.S003
{
    /// <summary>
    /// 003 — 待機あり / 引数なし / 返り値あり。
    ///
    /// ここで初めて await が「閉じた後」まで伸びる。名前がそれを示している
    /// (ShowAsync = Shown で返る / ShowForResultAsync = 閉じた後に結果つきで返る。
    ///  Android の startActivity / startActivityForResult と同じ分離)。
    ///
    /// 結果の受取口が await の戻り値しかないので、
    /// 「返り値あり × 待たない」は原理的に存在しない — だからパターンは 4 通り。
    /// </summary>
    public sealed class Sample003Bootstrap : MonoBehaviour
    {
        [SerializeField] private UiRegistryAsset _registry;
        [SerializeField] private string _key = "NameInput";
        [SerializeField] private Button _openButton;
        [SerializeField] private TMP_Text _resultText;

        private void Start()
        {
            _openButton.onClick.AddListener(OnOpenClicked);
        }

        private void OnDestroy()
        {
            // 自分が張った分だけ剥がす (RemoveAllListeners は同じ Button 上の
            // 他コンポーネントの購読を踏み潰すため使わない)
            _openButton.onClick.RemoveListener(OnOpenClicked);
        }

        private void OnOpenClicked() => AskNameAsync().Forget();

        private async UniTaskVoid AskNameAsync()
        {
            // 開く → 相手が閉じるまで待つ → 結果が戻り値で届く (イベント配線ゼロ)
            var result = await _registry.Host.ShowForResultAsync<string>(_key);

            // キャンセル系 (×ボタン / HideAsync / 再入無視) は全部 HasValue=false に統一。
            // だから呼び側の分岐はここ 1 個で済む。
            _resultText.text = result.HasValue
                ? $"Hello, {result.Value}"
                : "(canceled)";
        }
    }
}
