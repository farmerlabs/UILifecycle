namespace UiLifecycle
{
    /// <summary>
    /// 引数は受け取るが、結果は返さない UI のための基底。
    ///
    /// 型引数は「末尾から省略すると Unit が埋まる」という一貫した規則で減る:
    ///   UiEntryPoint&lt;TArgs, TResult&gt;  … 引数あり・結果あり
    ///   UiEntryPoint&lt;TArgs&gt;           … 引数あり・結果なし (= &lt;TArgs, Unit&gt;)
    ///   UiEntryPoint                   … 引数なし・結果なし (= &lt;Unit, Unit&gt;)
    /// arity 1 が「引数あり」を意味するのは IUiEntryPoint&lt;in TArgs&gt; と同じ規約。
    ///
    /// 結果を返せないことは型で表現される (Close() は結果を取らない)。
    /// 呼び側は ShowAsync(key, args) で開く。
    /// </summary>
    public abstract class UiEntryPoint<TArgs> : UiEntryPoint<TArgs, Unit>
    {
        /// <summary>閉じる。結果を返さないので Cancel() との差は呼び側から観測されない</summary>
        protected void Close() => Close(Unit.Default);
    }
}
