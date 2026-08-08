using System.Collections.Generic;

namespace UiLifecycle
{
    /// <summary>
    /// Persistent / Cached の実体保持。key ごとに 1 インスタンス。
    /// 「動的×非破壊で UI が増え続ける」問題は key 単位に閉じることで消える。
    /// </summary>
    public sealed class UiInstanceStore
    {
        private readonly Dictionary<string, IUiEntryPoint> _instances = new();

        public bool TryGet(string key, out IUiEntryPoint entryPoint)
        {
            if (_instances.TryGetValue(key, out entryPoint))
            {
                // Destroy 済み (シーン遷移等) の残骸は無効扱い
                if (!UiObject.IsAlive(entryPoint))
                {
                    _instances.Remove(key);
                    entryPoint = null;
                    return false;
                }
                return true;
            }
            return false;
        }

        public void Set(string key, IUiEntryPoint entryPoint) => _instances[key] = entryPoint;

        public void Remove(string key) => _instances.Remove(key);
    }
}
