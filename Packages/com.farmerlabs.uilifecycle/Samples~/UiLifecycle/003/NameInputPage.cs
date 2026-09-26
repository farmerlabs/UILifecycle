using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UiLifecycle.Samples.S003
{
    /// <summary>
    /// 003 — 引数なし / 返り値あり のページ。
    ///
    /// 継承するのは UiEntryPointForResult&lt;TResult&gt; (= &lt;Unit, TResult&gt;)。
    /// 「先頭の TArgs を省く」形なので末尾省略 (UiEntryPoint&lt;TArgs&gt;) には乗らず、
    /// 別名で公開している。名前は呼び側の ShowForResultAsync&lt;TResult&gt;(key) と対応。
    ///
    /// 入力フォームは「渡すものはないが返すものはある」の典型。
    /// </summary>
    public sealed class NameInputPage : UiEntryPointForResult<string>
    {
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private Button _decideButton;

        /// <summary>引数がないので OnShow() も引数なし。Unit は現れない</summary>
        protected override void OnShow()
        {
            // 「再表示 = 初期状態」— Cached でも前回の入力は残らない
            _input.text = string.Empty;

            // コードで購読するのは型付きデータ (_input.text) が流れる決定側だけ。
            // キャンセルはデータが流れないので UiCancelButton (コード 0 行) に任せる。
            // 自分が張った分だけ剥がして張り直す (RemoveAllListeners は
            // 同じ Button 上の他コンポーネントの購読を踏み潰すため使わない)
            _decideButton.onClick.RemoveListener(OnDecide);
            _decideButton.onClick.AddListener(OnDecide);   // 結果を返して閉じる
        }

        private void OnDecide() => Close(_input.text);

        private void OnDestroy()
        {
            _decideButton.onClick.RemoveListener(OnDecide);
        }
    }
}
