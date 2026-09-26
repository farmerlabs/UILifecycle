namespace UiLifecycle
{
    /// <summary>
    /// 使用者に見せる相は「表示/非表示」の 4 つのみ。
    /// Absent / Creating / Destroying は寿命ポリシーの実装詳細として基盤の内側に沈める。
    ///
    /// 語彙は親側 API の Show / Hide に揃えてある (Showing→Shown→Hiding→Hidden)。
    /// Close / Cancel は子が出す「閉じ要求」の語で、相の名前には出さない
    /// — どの経路で閉じても相の遷移は同一だから。
    /// </summary>
    public enum UiPhase
    {
        Hidden,
        Showing,
        Shown,
        Hiding
    }
}
