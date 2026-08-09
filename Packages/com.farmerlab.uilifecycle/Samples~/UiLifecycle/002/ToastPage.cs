using TMPro;
using UnityEngine;

namespace UiLifecycle.Samples.S002
{
    /// <summary>
    /// 002 — 引数あり / 返り値なし のページ。
    ///
    /// 継承するのは UiEntryPoint&lt;TArgs&gt; (= &lt;TArgs, Unit&gt;)。
    /// 結果を返せないことが型に出ている: Close() は引数を取らない。
    /// (Unit を書く必要はない — 末尾の型引数を省略すると埋まる)
    /// </summary>
    public sealed class ToastPage : UiEntryPoint<ToastArgs>
    {
        [SerializeField] private TMP_Text _messageText;

        /// <summary>
        /// 表示のたびに呼ばれる (Start より前)。
        /// ここで毎回組み立て直すことが「再表示 = 初期状態」の実体。
        /// Policy が Cached/Persistent でも前回の文言は残らない。
        ///
        /// コードを書くのは型付きデータが流れる部分 (args.Message) だけ。
        /// OK ボタンの閉じはデータが流れないので UiCancelButton (コード 0 行) に任せる
        /// — 結果なしページでは Close() と Cancel は呼び側から観測上同一。
        /// </summary>
        protected override void OnShow(ToastArgs args)
        {
            _messageText.text = args.Message;
        }
    }
}
