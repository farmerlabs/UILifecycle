# 001 — no awaiting / no args / no result

**English** | [日本語](README.ja.md)

**Zero lines of code.** A settings screen, a credits page, a help dialog — any UI with no typed data flowing through it is covered here.

| Item | Detail |
|---|---|
| Caller | `UiShowButton` (= `ShowAsync(key)`) / `UiHideButton` (= `HideAsync(key)`) |
| How it closes | child: `UiCancelButton` (inside the page) / parent: `UiHideButton` (in the scene) |
| Page base class | `UiPage` |
| When the await returns | at Shown (enter transition complete). It does not wait for the close |

## What is in the folder

| | Contents |
|---|---|
| `Page.prefab` | `UiPage` on the root, `UiCancelButton` on `CloseButton` (no key — it finds its own page) |
| `UiRegistry.asset` | Key = `Dialog` / Policy = `Transient` / Kind = `Prefab` / Prefab = `Page.prefab` |
| `001_SampleScene.unity` | `UiShowButton` on `Button_Show`, `UiHideButton` on `Button_Hide` (both with Registry + Key = `Dialog`) |

## What to look for

- **There is not a single script.** Opening, closing, awaiting transitions, lifetime management and the re-entry gate are all in effect
- **All three components appear**: open = `UiShowButton` (parent), close = `UiHideButton` (parent) and `UiCancelButton` (child). These are the only three cells that exist (package README §4-4)
- `UiHideButton` is a **silent no-op** when its target is not shown — pressing it and seeing nothing happen is the normal case
- Because the Policy is `Transient`, the instance is destroyed on close and rebuilt when reopened
- For closing with a result, see 003 / 004

Opening from code is covered from 002 onward.