using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UiLifecycle.Samples.S002
{
    /// <summary>
    /// 002 — 待機なし / 引数あり / 返り値なし。
    ///
    /// 001 との差は「型付きデータが流れるか」だけ。流れるので per-page クラス
    /// (ToastPage) を書く — それが「コンストラクタを取り戻す」の実体。
    /// await が返る時点は 001 と同じ Shown。
    /// </summary>
    public sealed class Sample002Bootstrap : MonoBehaviour
    {
        [SerializeField] private UiRegistryAsset _registry;
        [SerializeField] private string _key = "Toast";
        [SerializeField] private Button _openButton;
        [SerializeField] private string _message = "Save completed";

        private int _count;

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

        private void OnOpenClicked() => OpenAsync().Forget();

        private async UniTaskVoid OpenAsync()
        {
            // 引数は「開くたび」に渡す。生成時 1 回ではないので、
            // 2 回目以降も OnShow(args) が走り、毎回この文言で組み立て直される。
            await _registry.Host.ShowAsync(_key, new ToastArgs($"{_message} ({++_count})"));

            Debug.Log("[002] Shown. 結果は誰も待っていない");
        }
    }
}
