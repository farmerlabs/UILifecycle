# 005 — choosing a LifetimePolicy

**English** | [日本語](README.ja.md)

**The same page opened through three keys that differ only in Policy.** This covers one axis orthogonal to 001–004: lifetime.

| Key | Policy | Instance |
|---|---|---|
| `Transient` | created per showing, destroyed on close | prefab |
| `Cached` | created once, reused afterwards (closing = hiding) | prefab |
| `Persistent` | resident in the scene; the foundation neither creates nor destroys it | a GameObject in the scene plus `UiSceneAnchor` |

## What is in the folder

| | Contents |
|---|---|
| `PolicyProbePage.cs` | displays "instance #n / shown xN" (n counted from Awake, N from OnShow) |
| `Page.prefab` | `PolicyProbePage` on the root, `UiCancelButton` on the close button |
| `UiRegistry.asset` | the three entries above |
| `005_SampleScene.unity` | a `UiShowButton` / `UiHideButton` pair per key. The Persistent page sits in the scene and registers through `UiSceneAnchor` |

## What to look for

- **`Transient`**: `instance #` advances on every open and `shown` stays at x1 — it is rebuilt each time
- **`Cached` / `Persistent`**: `instance #` is fixed and `shown` advances — the instance survives
- **`OnShow` runs every time under all three** (identical behavior; only memory and acquisition cost differ). This is why a `Cached` page never shows stale content
- `UiSceneAnchor` registers itself in `Awake`. **An inactive GameObject never runs `Awake`, so it never registers** — hide it initially with `_hideOnAwake` (on by default) instead