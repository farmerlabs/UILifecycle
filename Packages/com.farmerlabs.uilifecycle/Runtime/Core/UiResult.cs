namespace UiLifecycle
{
    /// <summary>
    /// 表示の結末 = 値 or キャンセル。
    /// ×ボタン / 戻る / 第三者による Hide / 再入無視 は HasValue = false。
    /// </summary>
    public readonly struct UiResult<TResult>
    {
        public bool HasValue { get; }
        public TResult Value { get; }

        private UiResult(bool hasValue, TResult value)
        {
            HasValue = hasValue;
            Value = value;
        }

        public static UiResult<TResult> Of(TResult value) => new(true, value);
        public static UiResult<TResult> Canceled() => new(false, default);
    }
}
