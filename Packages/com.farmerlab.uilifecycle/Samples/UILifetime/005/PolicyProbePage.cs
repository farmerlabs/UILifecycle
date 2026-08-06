using TMPro;
using UnityEngine;

namespace UiLifecycle.Samples.S005
{
    /// <summary>
    /// 005 — LifetimePolicy を可視化するページ。
    /// 「何体目のインスタンスか」(Awake 起点) と「何回表示されたか」(OnShow 起点) を出すだけ。
    ///
    /// Transient   : 開くたび instance が進む (毎回作り直し)
    /// Cached      : instance は固定で shown が進む (実体が残る)
    /// Persistent  : 同上 (シーン常駐。UiSceneAnchor で登録)
    ///
    /// どのポリシーでも OnShow は毎回走る — 使用者から見た挙動は同一で、差は寿命 (性能特性) だけ。
    /// </summary>
    public sealed class PolicyProbePage : UiEntryPoint
    {
        private static int s_instanceCount;

        [SerializeField] private TMP_Text _label;

        private int _instanceNumber;
        private int _shownCount;

        private void Awake() => _instanceNumber = ++s_instanceCount;

        protected override void OnShow()
        {
            _shownCount++;
            _label.text = $"instance #{_instanceNumber} / shown x{_shownCount}";
        }
    }
}
