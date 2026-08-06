namespace UiLifecycle.Samples.S002
{
    /// <summary>
    /// 表示引数。struct にしているのは「開くたびに作って渡す値」であり、
    /// 参照を保持して後から書き換えるものではないことを型で示すため。
    /// </summary>
    public readonly struct ToastArgs
    {
        public readonly string Message;
        public ToastArgs(string message) => Message = message;
    }
}
