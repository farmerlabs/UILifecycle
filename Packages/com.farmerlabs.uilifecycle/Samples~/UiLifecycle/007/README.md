# 007 — supplying the page from an additive scene

**English** | [日本語](README.ja.md)

**Acquire the page from a scene instead of a prefab.** This covers one axis orthogonal both to the 001–004 grid and to 005's lifetime: the means of acquisition.

| Key | Kind | Policy | Instance |
|---|---|---|---|
| `SceneDialog` | `AdditiveScene` | `Transient` | loads `007_PageScene` additively, unloads it on close |

The means of acquisition is a single `ProviderKind` value. **Neither the caller nor a bootstrap needs one line of code** (same as 001).

## What is in the folder

| | Contents |
|---|---|
| `UiRegistry.asset` | the entry above. Once `007_PageScene` exists, drop it into the `Scene` field (it mirrors into `Scene Name`) |
| `007_PageScene.unity` | `UiPage` on the root, with a Canvas and a `UiCancelButton` beneath. **No EventSystem, no Camera** |
| `007_SampleScene.unity` | `UiShowButton` on `Button_Show`, `UiHideButton` on `Button_Hide`. The EventSystem lives only here |

There is no C# file. **Both scenes must be added to Scenes In Build** in Build Settings, or it fails with `Scene couldn't be loaded`.

## What to look for

- **It is handled exactly like a prefab.** The caller knows only the key, and swapping `Kind` in the Registry exchanges prefab for scene. The allowed policies are the same as for a prefab (`Cached` / `Transient`)
- **This is why `ShowAsync` is a `UniTask` from the start.** Loading a scene cannot be done synchronously. The await that returned within the same frame for a prefab now takes real time
- **`Construct` still happens before `Start`** (as with a prefab), because `AdditiveSceneProvider` hides the root in `sceneLoaded` — after `Awake` and before `Start`. The raw state is never visible, not even for one frame
- **Pressing `Button_Hide` during the load is not swallowed.** No `EntryPoint` exists yet, so `UiHost` holds the request and applies it right after `Construct` — the same behavior as "Hide during the enter transition" in 006
- **The load itself is not interrupted.** Cleaning up a half-loaded scene would add to the return contract of `ShowAsync`, so requests are unified through the holding behavior above instead
- **`Persistent` cannot be chosen.** It means "already placed in the scene, so the foundation does not acquire it". To keep a scene UI resident, load it yourself at startup and use `UiSceneAnchor` (= `SceneObject` / `Persistent`)