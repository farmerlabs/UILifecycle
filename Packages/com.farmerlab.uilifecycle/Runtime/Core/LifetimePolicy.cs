namespace UiLifecycle
{
    /// <summary>
    /// インスタンスの寿命。軸は 1 本。
    /// 「表示: 静的/動的」×「非表示: 破壊/非破壊」の 2×2 にしないのは、
    /// 静的×破壊 (一度で Missing) を型として表現不能にするため。
    /// 3 者は Construct を毎回呼ぶため使用者から見た挙動が同一で、
    /// 差はメモリと調達コストのみ (= 性能特性の選択)。
    /// </summary>
    public enum LifetimePolicy
    {
        /// <summary>シーンに配置。破棄しない (UiSceneAnchor で登録される)</summary>
        Persistent,

        /// <summary>初回だけ調達、以降再利用、破棄しない (遅延静的)</summary>
        Cached,

        /// <summary>表示ごとに調達、非表示で解放</summary>
        Transient
    }
}
