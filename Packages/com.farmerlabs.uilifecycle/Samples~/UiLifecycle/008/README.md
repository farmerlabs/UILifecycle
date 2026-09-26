# 008 — plugging in acquisition with a custom provider

**English** | [日本語](README.ja.md)

**The axis for plugging in acquisition the foundation does not provide.** The smallest useful example is "instantiate under a given parent".
Two keys share one prefab and one policy, and **appear in different places**.

| Key | Kind | Policy | CustomProviderId | Parent |
|---|---|---|---|---|
| `Dialog_Left` | `Custom` | `Transient` | `Anchor_Left` | `Anchor_Left` (a Transform in the scene) |
| `Dialog_Right` | `Custom` | `Transient` | `Anchor_Right` | `Anchor_Right` (likewise) |

The two Registry entries are identical apart from `CustomProviderId`. **The parent is decided outside the Registry.**

## What is in the folder

| | Contents |
|---|---|
| `Sample008Bootstrap.cs` | registers two providers in `Awake`: `RegisterProvider(id, new PrefabProvider(prefab, anchor))` |
| `FadePresenter.cs` | the same fade as 006 (`CanvasGroup.alpha`) |
| `Page.prefab` | `UiPage` + `CanvasGroup` + `FadePresenter` on the root, `UiCancelButton` on `CloseButton` (no code on the page side) |
| `UiRegistry.asset` | the two entries above. **The Prefab fields are empty** — the bootstrap holds the prefab reference |
| `008_SampleScene.unity` | `Anchor_Left` / `Anchor_Right` under the Canvas, `UiShowButton` on `Button_Show_Left` / `Button_Show_Right` |

Wire the bootstrap's `Prefab`, `Left Anchor` and `Right Anchor` in the inspector.

## What to look for

- **Changing one `Kind` value swaps the acquisition route** (the same point as 007, repeated). The caller still knows only the key, and `UiShowButton` still works with zero code
- **Custom is not reserved for heavy implementations.** Here it simply constructs `PrefabProvider` — the package's own implementation. What you plug in is a means of acquisition, not a dependency
- **The parent cannot live in the Registry.** A parent is a `Transform` in a scene, and the Registry is a ScriptableObject — an asset — so it cannot hold a scene reference. Adding a field would mean inventing a second self-registration mechanism after `UiSceneAnchor`. Using the plug-in point avoids inventing anything
- **Transitions are orthogonal to acquisition.** `FadePresenter` is carried over unchanged from 006, and acquiring through Custom does not affect it. Being `Transient`, the instance is released after the exit fade completes
- **Register in `Awake` every time.** `Host` is a shared instance created lazily by the Registry ScriptableObject, but it is discarded when play starts — a previous registration never survives, even with domain reload disabled
- `RegisterProvider` only has to run before the first Show. `Awake` is used because it is reliably earlier than a `UiShowButton` click
- **Only `Cached` and `Transient` are allowed.** `Persistent` means "the foundation does not acquire it", so it cannot combine with Custom (see 005)