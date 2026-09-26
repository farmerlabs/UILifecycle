using System;
using System.ComponentModel;

namespace UiLifecycle
{
    /// <summary>
    /// 「値がない」ことを表す型。文脈フリー層 (UiPage / 引数なし ShowAsync) の
    /// TArgs / TResult に内部で詰められる。
    /// 使用者がこの型を書く機会はない (非ジェネリック UiEntryPoint / UiPage が包んで見せない)。
    /// UniTask の AsyncUnit を流用しないのは、あちらの概念が「非同期の単位」であり
    /// 「引数がない」とは別物のため (概念の借り物をしない)。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct Unit : IEquatable<Unit>
    {
        public static readonly Unit Default = default;

        public bool Equals(Unit other) => true;
        public override bool Equals(object obj) => obj is Unit;
        public override int GetHashCode() => 0;
        public override string ToString() => "()";

        public static bool operator ==(Unit left, Unit right) => true;
        public static bool operator !=(Unit left, Unit right) => false;
    }
}
