using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UiLifecycle.Samples.S004
{
    /// <summary>
    /// 004 — 引数あり / 返り値あり のページ。全部入り。
    ///
    /// TArgs = 開くときに受け取るもの / TResult = 閉じるときに返すもの。
    /// 開閉は「対称」ではなく in / out — 下りが args、上りが result で、
    /// 上りの受け手は「開いた本人」に決まっている。だから結果は戻り値でよい。
    /// </summary>
    public sealed class ItemDetailPage : UiEntryPoint<ItemDetailArgs, string>
    {
        [SerializeField] private TMP_Text _itemIdText;
        [SerializeField] private Button _closeButton;

        protected override void OnShow(ItemDetailArgs args)
        {
            _itemIdText.text = args.ItemId.ToString();

            // コードで購読するのは確定 (Close(result)) を作る決定側だけ。
            // キャンセルはデータが流れないので UiCancelButton (コード 0 行) に任せる。
            // 自分が張った分だけ剥がして張り直す (RemoveAllListeners は
            // 同じ Button 上の他コンポーネントの購読を踏み潰すため使わない)
            _closeButton.onClick.RemoveListener(OnDecide);
            _closeButton.onClick.AddListener(OnDecide);   // 確定して閉じる
        }

        private void OnDecide() => Close("decided");

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(OnDecide);
        }
    }
}
