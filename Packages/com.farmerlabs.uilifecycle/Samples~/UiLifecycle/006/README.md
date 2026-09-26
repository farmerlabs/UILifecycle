# 006 — swapping the transition

**English** | [日本語](README.ja.md)

**The same page opened through four keys that differ only in their presenters.** This covers one axis orthogonal both to the 001–004 grid and to 005's lifetime: the transition.

| Key | Presenters | What you see |
|---|---|---|
| `None` | 0 | falls back to `ImmediateTransitionPresenter` — appears instantly |
| `Fade` | 1 | that one is used (0.25 s) |
| `Parallel` | 2, `Parallel` | fade and scale at the same time (0.25 s) |
| `Sequential` | 2, `Sequential` | fade, then scale (0.5 s) |

Presenters are **collected automatically from the EntryPoint's own GameObject**. The "0 / 1 / several" above is
literally the number of components, and it appears neither in the Registry nor on the caller side (a key still maps 1:1 to a UI).

## What is in the folder

| | Contents |
|---|---|
| `FadePresenter.cs` | drives a `CanvasGroup` alpha between 0 and 1. Derives from `UiTransitionPresenterBehaviour` and writes two methods |
| `ScalePresenter.cs` | drives `localScale` between 0.8 and 1. A second presenter, so that "two on one GameObject" can be shown |
| `Page_None.prefab` | `UiPage` only on the root, `UiCancelButton` on the close button |
| `Page_Fade.prefab` | the above plus `FadePresenter` (`_group` is the root's `CanvasGroup`) |
| `Page_Parallel.prefab` | plus `ScalePresenter`. `Presenter Mode` on `UiPage` = `Parallel` |
| `Page_Sequential.prefab` | a prefab variant of `Page_Parallel`, overriding only `Presenter Mode` = `Sequential` |
| `UiRegistry.asset` | the four entries above (all `Transient` / `Prefab`). **The prefab references are empty** — assign them in the inspector after creating the four prefabs |
| `006_SampleScene.unity` | a `Button_Show` / `Button_Hide` pair per key |

`Sequential` plays in **component order** (put Fade above and the fade runs first).

## What to look for

- **It works with zero presenters** (`None`). A product with no transitions at all can adopt the foundation as is
- **A composition is still an `IUiTransitionPresenter`.** `UiHost` cannot tell it from a single presenter; the count stays inside the EntryPoint
- **The instance never disappears before the exit transition finishes.** A `Transient` page is released after `ExitAsync` is awaited, so it cannot vanish mid-fade
- **Pressing `Button_Show` during the exit transition is ignored.** The re-entry gate does not open until `Hidden`
- **Pressing `Button_Hide` during the enter transition plays the entrance to completion first, then exits.** `RequestCancel()` only releases the wait; it does not cut a transition that is playing
- The entrance's initial values (alpha = 0, scale = 0.8) are assigned **before the first await**. `PlayEnterAsync` is called right after `SetActive(true)`, so this prevents the finished state from flashing for one frame