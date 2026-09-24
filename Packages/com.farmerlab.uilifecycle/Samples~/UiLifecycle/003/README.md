# 003 — awaiting / no args / result

**English** | [日本語](README.ja.md)

**The first sample where the await extends past the close.** An input form is the archetype of "nothing to pass in, something to return".

| Item | Detail |
|---|---|
| Caller | `Sample003Bootstrap` → `ShowForResultAsync<string>(key)` |
| How it closes | commit = `Close(_input.text)` (code) / cancel = `UiCancelButton` (child), `UiHideButton` (parent) |
| Page base class | `UiEntryPointForResult<string>` (= `<Unit, string>`; you never write `Unit`) |
| When the await returns | **after the page has closed**, with a result |

The difference from `ShowAsync` is when the await returns, and that difference is in the name
(the same split as Android's `startActivity` / `startActivityForResult`).
The only place a result can be received is the return value of the await — which is why "result, without awaiting" cannot exist.

## What is in the folder

| | Contents |
|---|---|
| `NameInputPage.cs` | `OnShow` initializes the input field and subscribes only the decide button (`Close(_input.text)`) |
| `Page.prefab` | `NameInputPage` on the root, `Button_Ok` assigned to `_decideButton`, `UiCancelButton` on `Button_Cancel` |
| `UiRegistry.asset` | Key = `NameInput` / Policy = `Transient` / Kind = `Prefab` |
| `003_SampleScene.unity` | `Button_Show` plus `Sample003Bootstrap` (with a TMP_Text for the result), `UiHideButton` on `Button_Hide(Cancel)` |

## What to look for

- **Only the child can produce a commit**: `HasValue = true` ⇔ the page called `Close(result)`. That is why code is written only on the decide path
- **Every cancellation path returns the same shape**: `UiCancelButton` (child), `UiHideButton` (parent) and ignored re-entry all give `HasValue = false`. The caller needs one branch, `if (result.HasValue)`
- Press `Button_Hide(Cancel)` while typing and you can watch **the waiting Bootstrap's await return with `HasValue = false`**
- An await that never returns is forbidden as an invariant