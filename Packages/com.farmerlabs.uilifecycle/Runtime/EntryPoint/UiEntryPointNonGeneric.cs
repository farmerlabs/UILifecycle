namespace UiLifecycle
{
    /// <summary>
    /// 型付きデータが流れない UI のための基底 (文脈フリー層)。
    /// UiEntryPoint&lt;Unit&gt; (= &lt;Unit, Unit&gt;) を包み、使用者には引数なしの OnShow() だけを見せる
    /// (Unit は内部実装詳細。使用者がこの型を書く機会を作らない)。
    ///
    /// 境界は「型付きデータが流れるか」:
    /// 流れないなら本クラス (またはコード 0 行の UiPage)、
    /// 流れるなら UiEntryPoint&lt;TArgs, TResult&gt; を書く (それが「コンストラクタを取り戻す」の実体)。
    /// 片方向だけ流れるなら UiEntryPoint&lt;TArgs&gt; / UiEntryPointForResult&lt;TResult&gt;。
    /// </summary>
    public abstract class UiEntryPoint : UiEntryPoint<Unit>
    {
        protected sealed override void OnShow(Unit args) => OnShow();

        /// <summary>
        /// 表示のたびに呼ばれる。組み立て直すものが何もなければ override 不要。
        /// </summary>
        protected virtual void OnShow() { }
    }
}
