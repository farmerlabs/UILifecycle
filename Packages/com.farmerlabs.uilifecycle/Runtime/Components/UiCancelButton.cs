using UnityEngine;
using UnityEngine.UI;

namespace UiLifecycle
{
    /// <summary>
    /// クリックで自分の属するページをキャンセルする使い回しボタン (子側 / 文脈フリー層)。
    /// 親方向の IUiEntryPoint を拾うため、Key も Registry も要らない。
    ///
    /// 「Close」ではなく「Cancel」なのは、このボタンが確定 (Close(result)) を作れないから。
    /// 結果はページが自分の状態から組み立てるもので、Inspector の設定だけでは作れない。
    /// 結果を返して閉じたい場合はコードを書く層に上がる (ページ内で Close(result) を呼ぶ)。
    ///
    /// 親側から key で閉じたい場合は UiHideButton (HideAsync) を使う。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class UiCancelButton : MonoBehaviour
    {
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
            var entryPoint = GetComponentInParent<IUiEntryPoint>(true);
            if (entryPoint == null)
            {
                Debug.LogError($"UiCancelButton '{name}' : 親に IUiEntryPoint が見つからない。ページの Root 配下に置くこと。", this);
                return;
            }
            entryPoint.RequestCancel();
        }
    }
}
