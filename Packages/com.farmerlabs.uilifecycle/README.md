# Farmer Lab UI Lifecycle — Setup and Usage

**English** | [日本語](README.ja.md)

A foundation that standardizes the presence states of a UI (Hidden / Showing / Shown / Hiding) and lets you plug "something that reports completion" into each transition point.

What it really does: **it gives UI construction back the constructor that Unity components lack.**

---

## 1. Requirements

| Item | Requirement |
|---|---|
| Unity | 2022.3 or later |
| Dependencies | **UniTask 2.5.11 or later — nothing else** |

## 2. Installation

Order matters. **Follow 2-1, then 2-2.**

### 2-1. Get UniTask (required)

This package depends on **UniTask 2.5.11 or later**. Choose either A or B.

#### 2-1-A. Via OpenUPM (recommended)

Add a scoped registry to your project's `Packages/manifest.json`:

```jsonc
{
  "dependencies": { /* leave as is */ },
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [ "com.cysharp.unitask" ]
    }
  ]
}
```

That is all. UniTask is pulled in **automatically in step 2-2** through this package's `dependencies`. No manual install needed.

> This package itself is not distributed on OpenUPM. The scoped registry is only there to resolve UniTask.

#### 2-1-B. Via git URL (alternative)

Use this if OpenUPM is not an option. Two rules apply:

- **Version must be 2.5.11 or later** (the minimum this package requires)
- **It must be installed before this package (2-2)**

Package Manager > `+` > *Add package from git URL...*

```
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

> If you install this package first, UPM will look for UniTask in a registry and fail to resolve it.

### 2-2. Install this package

Package Manager > `+` > *Add package from git URL...*

```
https://github.com/Farmer0116/UILifecycle.git?path=Packages/com.farmerlab.uilifecycle#v0.1.1
```

`?path=` points to the package folder inside the repository; `#v0.1.1` is the tag to fetch.
Omitting the tag fetches the tip of the default branch, which can change without notice. **Always pin a tag.**

Or write it directly in `manifest.json`:

```jsonc
{
  "dependencies": {
    "com.farmerlab.uilifecycle": "https://github.com/Farmer0116/UILifecycle.git?path=Packages/com.farmerlab.uilifecycle#v0.1.1"
  }
}
```

If you chose 2-1-A, UniTask is fetched automatically at this point.

Open Unity; once it compiles, installation is done.

### 2-3. Import the samples (optional)

> **The samples additionally require TextMeshPro** (`com.unity.textmeshpro`), and will not compile without it.
> The runtime package itself does not use TextMeshPro, so it is not declared as a dependency — install it from Package Manager if your project does not already have it.

Package Manager > select this package > **Samples** tab > **Import** on *Sample UI Lifecycle*

It expands to:

```
Assets/Samples/Farmer Lab UI Lifecycle/0.1.1/Sample UI Lifecycle/
```

Numbered folders 001 through 008 each hold a scene and a README for one feature. The quickest start is to open the `001` scene and press Play.

---

## 3. Vocabulary rules

Every API name in this package can be read off these four lines.

> - **Show / Hide** are commands the **parent** (the layer that only knows a key) issues to the Host.
> - **Close / Cancel** are close requests issued by the **child** (the page itself). Close commits, Cancel withdraws.
> - **Awaiting vs. not awaiting** is expressed solely by the difference between `ShowForResultAsync` and `ShowAsync`. Neither the page type nor the closing vocabulary takes part.
> - **Only the child can produce a commit.** The parent does not know `TResult`, so closing from the parent is always a cancellation.

| Term | Layer | Subject | Concretely |
|---|---|---|---|
| Show / Hide | Host's public API | the caller that only knows a key (parent) | `ShowAsync`, `ShowForResultAsync`, `HideAsync`, `IsShown` |
| Close / Cancel | inside the page | the page itself (child) | `Close(result)`, `Cancel()` |

The phase enum (`UiPhase`) follows the parent's vocabulary too: `Showing → Shown → Hiding → Hidden`.
Because the phase transitions are identical no matter which path closed the page, Close / Cancel never appear in phase names.

## 4. The whole picture — the minimum you need to choose between APIs

### 4-1. Page kinds — only two axes: arguments and result

| Base class | Args | Result | How the child closes |
|---|:-:|:-:|---|
| `UiPage` | – | – | n/a (zero lines of code; closed from outside or by a button) |
| `UiEntryPoint` | – | – | `Close()` / `Cancel()` |
| `UiEntryPoint<TArgs>` | yes | – | `Close()` / `Cancel()` |
| `UiEntryPointForResult<TResult>` | – | yes | `Close(result)` / `Cancel()` |
| `UiEntryPoint<TArgs, TResult>` | yes | yes | `Close(result)` / `Cancel()` |

### 4-2. "Await or not" is not a property of the page — the two are orthogonal

**There is no fire-and-forget API.** There are only two answers to "how far do you wait?":

| API | When the await returns |
|---|---|
| `ShowAsync` | at **Shown** (the enter transition has finished; closing and release run in the background) |
| `ShowForResultAsync` | at **Closed** (after the exit transition and release have finished, with a result) |

Awaiting or not is chosen **per call by the caller**; the page type plays no part:

- You may open a page that has a result with `ShowAsync` (the result is discarded)
- You may open a page with no result with `ShowForResultAsync` (you just await "it closed")

The implication runs one way only: receiving a result ⇒ you necessarily waited until it closed. The converse does not hold.

### 4-3. Four ways to close, two observable outcomes

| Path | Actor | Commit / Cancel | `UiResult` |
|---|---|---|---|
| `Close(result)` | child | commit | `HasValue = true` |
| `Cancel()` | child | cancel | false |
| `HideAsync(key)` | parent | cancel | false |
| Re-entry ignored (Show on a key already shown) | parent | cancel | false |

`HasValue = true` ⇔ **the child called `Close(result)`** holds exactly.
Conversely **`HasValue = false` has three causes** (child Cancel / parent Hide / ignored re-entry), and the caller cannot tell them apart — deliberately not providing that axis is a design decision.

On an ignored re-entry, the call that gets cancelled is **the second call, not the page already on screen**. The existing display is untouched. The gate stays closed until the page has fully closed (release complete).

### 4-4. Component buttons — only three cells exist

Cross "open / close" with "parent / child" and only three cells can be expressed with `[SerializeField]` alone. The rest are structurally empty: the moment typed data flows, you move up to the layer where you write code.

| Component | API | Where it goes | Required setup |
|---|---|---|---|
| `UiShowButton` | `ShowAsync(key)` | caller side (parent) | Registry + Key |
| `UiHideButton` | `HideAsync(key)` | caller side (parent) | Registry + Key |
| `UiCancelButton` | the page's own `Cancel` | under the page Root (child) | none |

The empty cells and why:

- open / with args, awaited → typed args and results cannot live in the Inspector
- close / parent / **commit** → the parent does not know `TResult`. **Impossible in principle** (line 4 of §3)
- close / child / **commit** → the result is assembled by the child from its own state; the Inspector cannot build it

`UiCancelButton` is called "Cancel" rather than "Close" precisely because this button cannot produce a commit.

---

## 5. Quick start, tier 1 (zero lines of code)

A UI with no data flowing through it (opening a settings screen, closing a popup) works without writing a single line.

### 5-1. Make a page

Attach **`UiPage`** to the root of a prefab. That is all. (It can be under a Canvas or in 3D; the foundation knows nothing about appearance.)

### 5-2. Make a Registry

Right-click in the Project view > **Create > UiLifecycle > Registry**.

Add an entry in the Inspector:

| Field | Value |
|---|---|
| Key | `Settings` (a noun = the UI's ID) |
| Policy | `Transient` (created and destroyed per showing) |
| Kind | `Prefab` |
| Prefab | the prefab from 5-1 |

### 5-3. Wire up the buttons

- To open: attach **`UiShowButton`** to any `Button` and set Registry and Key in the Inspector
- To close (child): attach **`UiCancelButton`** to a `Button` inside the page — nothing else (no Key; it finds its own page)
- To close (parent): attach **`UiHideButton`** to a `Button` outside the page and set Registry and Key (nothing happens if the target is not shown)

That gives you open/close, transition awaiting, lifetime management, and the re-entry gate, all in effect.

To open from code, it is still one line with no arguments:

```csharp
await registry.Host.ShowAsync("Settings");   // returns at Shown (enter transition complete)
```

`registry.Host` is the shared Host held by the Registry ScriptableObject (lazily created, reset on domain reload).
You may `new UiHost(registry)` yourself, but the re-entry gate is per Host, so do not mix the two on the same Registry.

---

## 6. Quick start, tier 2 (write code only when data flows)

The boundary is "**does typed data flow?**" If it does, write the page's type — that is what "giving back the constructor" means in practice.

### 6-1. Write the page

Pick a base class from the table in §4-1. You write `OnShow`, plus `Close` / `Cancel` where you want to close.

```csharp
using UiLifecycle;
using UnityEngine;

public readonly struct ItemDetailArgs
{
    public readonly int ItemId;
    public ItemDetailArgs(int itemId) => ItemId = itemId;
}

// TArgs = what it receives when opened / TResult = what it returns when closed
public sealed class ItemDetailPage : UiEntryPoint<ItemDetailArgs, string>
{
    // Called on every showing. Rebuild here each time: "re-shown = initial state"
    protected override void OnShow(ItemDetailArgs args)
    {
        // build the view from args.ItemId
    }

    // call these from buttons
    public void OnDecideButton() => Close("decided");   // commit and close
    public void OnBackButton()   => Cancel();           // withdraw and close
}
```

Registering it in the Registry is the same as tier 1.

**No base class ever gives you a reason to write `Unit`.** Type arguments fill in when omitted from the end (`UiEntryPoint<TArgs>` = `<TArgs, Unit>`).
Only "no args, with result" would require omitting from the front, which trailing omission cannot express, so it is exposed under the alias `UiEntryPointForResult<TResult>` (matching `ShowForResultAsync<TResult>(key)` on the caller side).

### 6-2. Open it and receive the result

The moment the await returns is visible in the name — the same split as Android's `startActivity` / `startActivityForResult` (§4-2):

```csharp
using UiLifecycle;
using Cysharp.Threading.Tasks;

public sealed class Bootstrap : MonoBehaviour
{
    [SerializeField] private UiRegistryAsset _registry;
    private UiHost _host;

    private void Awake() => _host = new UiHost(_registry);

    public async UniTaskVoid OpenDetail(int itemId)
    {
        // open -> wait until it closes -> the result arrives as a return value (no event wiring)
        var result = await _host.ShowForResultAsync<ItemDetailArgs, string>(
            "ItemDetail", new ItemDetailArgs(itemId));

        if (result.HasValue)
            Debug.Log($"decided: {result.Value}");   // the child called Close(result)
        else
            Debug.Log("cancelled");                  // child Cancel / parent Hide / ignored re-entry
    }
}
```

`UiHost` is a plain C# class. How you expose it (DI, singleton, direct `new`, `registry.Host`) is left to your architecture.

When you want to pass arguments but do not need the result:

```csharp
await _host.ShowAsync("ItemDetail", new ItemDetailArgs(itemId));
// returns at Shown. Closing and release are handled in the background
```

When you need no arguments but do want a result (e.g. an input field built at runtime):

```csharp
var result = await _host.ShowForResultAsync<string>("InputForm");
// the page derives from UiEntryPointForResult<string> and returns via Close(text)
```

Closing from the parent (always a cancellation, §4-3):

```csharp
await _host.HideAsync("ItemDetail");  // awaits the exit transition and release
```

---

## 7. UI placed in a scene (Persistent)

A UI placed directly in a scene is driven by the same `ShowAsync` / `HideAsync`.

1. Attach `UiPage` (or a class deriving from `UiEntryPoint`) to the GameObject in the scene
2. Add **`UiSceneAnchor`** to the same GameObject and set Registry and Key
3. In the Registry, set Key / `Policy = Persistent` / `Kind = SceneObject` (no prefab)

`UiSceneAnchor` registers itself with the Registry in `Awake`. With `_hideOnAwake` (on by default) it hides itself after registering.

> Note: **an inactive GameObject never runs `Awake`, so it never registers.** If you want it hidden initially, leave it active and use `_hideOnAwake`.

## 8. Policy x Kind combinations

| Policy | Meaning | Allowed Kind |
|---|---|---|
| `Persistent` | resident in the scene; the foundation neither creates nor destroys it | `SceneObject` |
| `Cached` | acquired on first use, reused afterwards (closing = hiding) | `Prefab` / `AdditiveScene` / `Custom` |
| `Transient` | acquired per showing, released on close | `Prefab` / `AdditiveScene` / `Custom` |

Invalid combinations are rejected at runtime with an exception that states the reason (static x destructive cannot be expressed).

**`Persistent` is `SceneObject`-only because Policy also decides whether the foundation acquires the instance.**
"Already placed in the scene" means "do not acquire", so pairing it with an acquiring Kind contradicts itself.
Blocking it costs no expressiveness: for a resident UI use `SceneObject` + `Persistent`, and for lazy residency `Cached` covers the same behavior.

Under every Policy, **`OnShow(args)` is called on every showing.** The only difference is lifetime.

### 8-1. Kind — the means of acquisition (orthogonal to Policy)

| Kind | Acquisition | Notes |
|---|---|---|
| `SceneObject` | none (`UiSceneAnchor` self-registers) | `Persistent` only |
| `Prefab` | `Instantiate` | completes synchronously |
| `AdditiveScene` | load the scene additively, then find `IUiEntryPoint` from the root | attach the scene to `Scene`. **Must also be registered in Build Settings** |
| `Custom` | an implementation you plug in via `UiHost.RegisterProvider(id, provider)` | acquisition needing external dependencies (Addressables etc.) goes here |

`Scene` is an Inspector field you drop a scene asset into; its name is mirrored into the runtime `Scene Name` field
(`SceneAsset` is an Editor-only type that cannot survive into a build, so the runtime truth is always `Scene Name`).
The attachment exists to follow renames and prevent typos — **it is not a substitute for Build Settings registration.** That is a Unity constraint.

Operational constraints specific to `AdditiveScene` (they do not apply to `Prefab`):

- **Do not put an EventSystem or Camera in a UI scene** (duplicates break input)
- Unload only destroys GameObjects belonging to that scene. **Do not leak objects created by a UI scene into other scenes**
- **One page per scene.** Do not assign the same scene name to more than one key

## 9. Plugging in transitions (optional)

The foundation's only contract is "report completion through a `UniTask`". The implementation can be Feel, DOTween, USS, hand-written — anything.

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using UiLifecycle;

public sealed class FadePresenter : UiTransitionPresenterBehaviour
{
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private float _duration = 0.2f;

    public override async UniTask PlayEnterAsync(CancellationToken ct)
    {
        for (var t = 0f; t < _duration; t += Time.deltaTime)
        {
            _group.alpha = t / _duration;
            await UniTask.Yield(ct);
        }
        _group.alpha = 1f;
    }

    public override async UniTask PlayExitAsync(CancellationToken ct)
    {
        for (var t = 0f; t < _duration; t += Time.deltaTime)
        {
            _group.alpha = 1f - t / _duration;
            await UniTask.Yield(ct);
        }
        _group.alpha = 0f;
    }
}
```

Attaching it to the same GameObject as the EntryPoint is enough; they are collected automatically:

- 0 presenters → completes immediately (it works with no transitions at all)
- 1 presenter → that one is used
- several → composed. Choose `Parallel` or `Sequential` via `Presenter Mode` on the EntryPoint's Inspector

**Only the EntryPoint's own GameObject is scanned** (`GetComponents`; children are not). A presenter on a child is silently ignored.
What it animates is whatever the `SerializeField` references point at, which may well be a child — so the usual shape is "**component on the root, targets on the parts**".
`Sequential` order follows the component order on the root, so rearranging the hierarchy for layout reasons does not change playback order.

Release happens only after the exit transition has been awaited, so an object never disappears mid-fade.

## 10. Behavior worth knowing

- **Construct happens before Start**, regardless of how the instance was supplied. "From `Start` onward, the arguments are in place" is all you need to remember. Subscribers use `OnConstructed`, which replays immediately on subscription if it has already fired, so nothing is missed
  - This holds because of the `IUiInstanceProvider` contract: **an instance is hidden immediately after acquisition.** `Prefab` satisfies it automatically since `Instantiate` completes synchronously; acquisitions that span frames (`AdditiveScene`, Addressables) satisfy it by hiding explicitly. Honor this when writing your own provider — otherwise `Start` runs ahead and the raw state becomes visible before the enter transition sets its initial values
- **Hide works during acquisition too.** A `HideAsync` issued while no instance exists yet is held by `UiHost` and applied right after `Construct`. Like "Hide during the enter transition", it opens fully and then closes. The load itself is not interrupted
- **Re-entry is ignored.** Showing the same key while it is already shown does not open a second copy. The await still **always returns** (immediately, with `HasValue = false`; §4-3)
- Showing two copies of the same key simultaneously is not supported (key = the UI's ID)
- **You never need to write `Unit`.** The caller side is covered by four methods (`ShowAsync` / `ShowForResultAsync`, with and without args) and the page side by four base classes (the table in §4-1). `Unit` is an implementation detail
- **A page's `TResult` is a capability, not an obligation.** `ShowAsync<TArgs>` does not ask for a result, so you may open a `UiEntryPoint<TArgs, TResult>` page and discard the result. Whether to receive it is the caller's choice (§4-2)

## 11. Running the tests

**Window > General > Test Runner > PlayMode** tab → `UiLifecycle.Tests` → Run All (11 tests).

The test assembly is constrained by `UNITY_INCLUDE_TESTS`, so it is never included in a build.

## 12. Current limitations

- Rollback on `CancellationToken` cancellation is minimal. **Acquisition (scene loading etc.) is not interrupted midway** — cleaning up a half-loaded scene would add to the return contract of `ShowAsync`, so close requests are instead unified through the holding behavior in §10
- `Kind = Custom` (Addressables and other acquisitions with external dependencies) only provides the plug-in point `UiHost.RegisterProvider(id, provider)`. The provider implementation lives in your product
- Generating key constants (UiKeys.g.cs plus a `[UiKey]` dropdown) is not implemented. For now, write key strings literally

## 13. License

MIT License. See [LICENSE.md](LICENSE.md).
