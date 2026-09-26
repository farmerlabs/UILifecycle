# UiLifecycle samples

**English** | [日本語](README.ja.md)

**Read them in order: each one adds a single element.** They are arranged by reading order, not as a cartesian product of the axes.

| # | Awaits | Args | Result | Caller API | Page base class |
|---|---|---|---|---|---|
| [001](001/) | no | no | no | `ShowAsync(key)` | `UiPage` (zero lines of code) |
| [002](002/) | no | yes | no | `ShowAsync(key, args)` | `UiEntryPoint<TArgs>` |
| [003](003/) | yes | no | yes | `ShowForResultAsync<TResult>(key)` | `UiEntryPointForResult<TResult>` |
| [004](004/) | yes | yes | yes | `ShowForResultAsync<TArgs, TResult>(key, args)` | `UiEntryPoint<TArgs, TResult>` |

From 005 on, each sample covers one axis **orthogonal to that grid** (one subject per sample).
Every one of them combines with any quadrant of 001–004.

| # | Axis | What it compares |
|---|---|---|
| [005](005/) | lifetime | `LifetimePolicy` = `Transient` / `Cached` / `Persistent` |
| [006](006/) | transitions | 0, 1 or several presenters (`Parallel` / `Sequential`) |
| [007](007/) | acquisition | `ProviderKind` = `AdditiveScene` (one Registry value apart from `Prefab`) |
| [008](008/) | acquisition | `ProviderKind` = `Custom` (plug acquisition in from outside; here, choosing the parent) |

## Why four, and not eight or six

There appear to be three axes (awaiting / args / result), but **awaiting and result are not independent**.
The only place a result can be received is the return value of the `await`, so:

- "**result, without awaiting**" cannot exist — there is nowhere to receive it
- "**no result, but awaiting**" has no motivation. If you want to wait until it closes but do not need a
  result, use 003 / 004 and look only at `HasValue` (every cancellation path is unified to `HasValue = false`)

What remains is **args or no args × awaiting or not** = 4.
That matches both the four methods on `IUiHost` and the four page base classes.

## Conventions shared by every sample

- **`Unit` appears nowhere.** Four caller methods and four page base classes cover every quadrant
- `registry.Host` is the shared `UiHost` held by the Registry ScriptableObject (created lazily).
  You may `new UiHost(registry)` yourself, but the re-entry gate is per Host, so do not mix the two on one Registry
- Each sample is **self-contained** (its own scene, Registry asset and prefab)
- `OnShow(args)` runs **on every showing**. Changing the Policy does not change that; it only changes memory
  use and acquisition cost

## Creating the scenes, prefabs and registries

Each folder's README has the steps. Everything that is not C# (`.unity`, `.prefab`, `.asset`) has to be
created in the Unity editor.

> The samples require **TextMeshPro** in addition to this package.