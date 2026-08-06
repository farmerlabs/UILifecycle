namespace UiLifecycle
{
    /// <summary>
    /// コード 0 行で使う EntryPoint。Prefab / シーンの Root に付けるだけ。
    /// データが流れず、開閉の標準化 (演出待ち・寿命・再入ゲート) だけ欲しい UI 向け。
    /// データが流れるようになったら UiEntryPoint&lt;TArgs, TResult&gt; へ書き換える。
    /// </summary>
    public sealed class UiPage : UiEntryPoint
    {
    }
}
