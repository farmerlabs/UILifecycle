# 004 — awaiting / args / result

**English** | [日本語](README.ja.md)

**Everything at once.** This is 003 with arguments added; the nature of the await is unchanged.

| Item | Detail |
|---|---|
| Caller | `Sample004Bootstrap` → `ShowForResultAsync<ItemDetailArgs, string>(key, args)` |
| How it closes | commit = `Close("decided")` (code) / cancel = `UiCancelButton` (child), `HideAsync` in code (parent) |
| Page base class | `UiEntryPoint<ItemDetailArgs, string>` |
| When the await returns | **after the page has closed**, with a result |

Opening and closing are not symmetric — they are in and out. Downward goes `args` (caller → UI); upward goes `result` (UI → whoever opened it).
Because the recipient of the upward direction is fixed to "whoever opened it", a return value is enough.

## What is in the folder

| | Contents |
|---|---|
| `ItemDetailArgs.cs` | the show arguments (a readonly struct) |
| `ItemDetailPage.cs` | `OnShow(args)` builds the view and subscribes only the decide button (`Close("decided")`) |
| `Page.prefab` | `ItemDetailPage` on the root, `Button_Close` assigned to `_closeButton`, `UiCancelButton` on `Button_Cancel` |
| `UiRegistry.asset` | Key = `ItemDetail` / Policy = `Transient` / Kind = `Prefab` |
| `004_SampleScene.unity` | `Button_Show` / `Button_Hide` plus `Sample004Bootstrap` (with a TMP_Text for the result) |

## What to look for

- Commit → `Result: decided`, cancel → `(canceled)`. The caller needs one branch, `if (result.HasValue)`
- **`Button_Hide` is the code version of `UiHideButton` from 001–003** (`HideAsync(key)` called from the Bootstrap). The waiting await returns with `HasValue = false` — closing from the parent is always a cancellation
- Changing the Policy from `Transient` to `Cached` **does not change the behavior** (both run `OnShow(args)` every time). Only memory use and acquisition cost differ. See 005 for choosing between them