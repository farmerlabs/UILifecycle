using System.Collections.Generic;

namespace UiLifecycle.Tests
{
    public readonly struct TestArgs
    {
        public readonly int Id;
        public TestArgs(int id) => Id = id;
    }

    /// <summary>テスト用ページ。ライフサイクルの呼び出し順を記録する</summary>
    public sealed class TestPage : UiEntryPoint<TestArgs, string>
    {
        public readonly List<string> EventLog = new();
        public int OnShowCount { get; private set; }
        public TestArgs? LastArgs { get; private set; }

        protected override void OnShow(TestArgs args)
        {
            OnShowCount++;
            LastArgs = args;
            EventLog.Add($"OnShow({args.Id})");
        }

        private void Awake() => EventLog.Add("Awake");
        private void Start() => EventLog.Add("Start");

        public void DoClose(string result) => Close(result);
        public void DoCancel() => Cancel();
    }
}
