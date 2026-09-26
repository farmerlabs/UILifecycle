# 002 — no awaiting / args / no result

**English** | [日本語](README.ja.md)

**001 with arguments added.** Typed data now flows, so this is the first sample that writes a page class.
Only the part that carries data is written (`args.Message`) — closing is still `UiCancelButton`, as in 001, with no code.

| Item | Detail |
|---|---|
| Caller | `Sample002Bootstrap` → `ShowAsync(key, args)` |
| How it closes | child: `UiCancelButton` (inside the page) / parent: `UiHideButton` (in the scene) |
| Page base class | `UiEntryPoint<ToastArgs>` (= `<ToastArgs, Unit>`; you never write `Unit`) |
| When the await returns | at Shown (enter transition complete), same as 001 |

## What is in the folder

| | Contents |
|---|---|
| `ToastArgs.cs` | the show arguments (a readonly struct) |
| `ToastPage.cs` | `OnShow(args)` just assembles the message |
| `Page.prefab` | `ToastPage` on the root, `UiCancelButton` on the OK button |
| `UiRegistry.asset` | Key = `Toast` / Policy = `Transient` / Kind = `Prefab` |
| `002_SampleScene.unity` | `Button_Show` plus `Sample002Bootstrap` (passes a numbered message each time), `UiHideButton` on `Button_Hide` |

## What to look for

- **Arguments are passed on every showing**, not once at creation. `OnShow(args)` runs again on the second and later showings, rebuilding from that showing's message
- Hammering the open button does nothing: **re-entry while shown is ignored**. The `await` still returns, so the caller never hangs
- Opening needs code (arguments are involved); closing does not (no data flows). The boundary between the two tiers is asymmetric between opening and closing
- For closing with a result, see 003 / 004. For choosing a Policy, see 005