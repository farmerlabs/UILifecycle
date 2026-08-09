namespace UiLifecycle
{
    /// <summary>
    /// 破棄済み Unity オブジェクトの判定を 1 箇所に集める。
    ///
    /// Destroy 後に == null が true を返すのは UnityEngine.Object 型として比較したときだけ。
    /// 本パッケージは実体を IUiEntryPoint / IUiTransitionPresenter で持ち回るため、
    /// 素の != null や ?. は参照比較に落ちて破棄済みを素通りさせる
    /// (次に Unity 側へ触れた瞬間 MissingReferenceException)。
    /// </summary>
    internal static class UiObject
    {
        /// <summary>
        /// 実体がまだ触れる状態か。null と破棄済みを同じ「無効」に畳む。
        /// Unity オブジェクトでない実装 (ImmediateTransitionPresenter 等) は常に有効。
        /// </summary>
        public static bool IsAlive(object instance)
        {
            if (instance is UnityEngine.Object unityObject) return unityObject != null;
            return instance != null;
        }
    }
}
