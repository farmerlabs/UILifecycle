namespace UiLifecycle
{
    /// <summary>複数演出を束ねる際の実行方式</summary>
    public enum ExecutionMode
    {
        /// <summary>全て同時に実行 (WhenAll)</summary>
        Parallel,

        /// <summary>順番に実行 (前の完了後に次)</summary>
        Sequential
    }
}
