namespace UiLifecycle
{
    /// <summary>
    /// 引数は要らないが、結果は返す UI のための基底
    /// (例: 入力フィールドを動的生成し、入力値だけを返すフォーム)。
    ///
    /// 「先頭の TArgs を省く」形になるため、末尾省略の規則
    /// (UiEntryPoint&lt;TArgs&gt; = &lt;TArgs, Unit&gt;) には乗らない。
    /// arity 1 は既に「引数あり」の意味で埋まっているので、別名で公開する。
    /// 名前は呼び側 API の ShowForResultAsync&lt;TResult&gt;(key) と対応させてある。
    /// </summary>
    public abstract class UiEntryPointForResult<TResult> : UiEntryPoint<Unit, TResult>
    {
        protected sealed override void OnShow(Unit args) => OnShow();

        /// <summary>
        /// 表示のたびに呼ばれる。ここで入力欄を組み立て直すことが「再表示 = 初期状態」の実体。
        /// </summary>
        protected virtual void OnShow() { }
    }
}
