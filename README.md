# UI Lifecycle for Unity

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity)](https://unity.com/)
[![Release](https://img.shields.io/github/v/tag/Farmer0116/UILifecycle?label=release&sort=semver)](https://github.com/Farmer0116/UILifecycle/releases)

**English** | [日本語](README.ja.md)

A foundation that standardizes the presence states of a UI (Hidden / Showing / Shown / Hiding) and lets you plug "something that reports completion" into each transition point.

What it really does: **it gives UI construction back the constructor that Unity components lack.**

```csharp
// open a page, wait until it closes, receive its result as a return value
var result = await host.ShowForResultAsync<ItemDetailArgs, string>(
    "ItemDetail", new ItemDetailArgs(itemId));

if (result.HasValue)
    Debug.Log($"decided: {result.Value}");
```

## Features

- **Four presence states, one vocabulary.** `Showing → Shown → Hidden` is the same no matter how a page was closed.
- **Arguments and results are typed.** A page declares what it takes and what it returns; the caller gets the result as a return value, with no event wiring.
- **Awaiting is the caller's choice.** `ShowAsync` returns at Shown; `ShowForResultAsync` returns after the page has closed and been released.
- **Zero code for simple UI.** `UiPage` plus three Inspector components cover open/close with no scripting.
- **Transitions are pluggable.** The only contract is "report completion through a `UniTask`" — Feel, DOTween, USS or hand-written all work.
- **Lifetime is declarative.** `Persistent` / `Cached` / `Transient` x `SceneObject` / `Prefab` / `AdditiveScene` / `Custom`.

## Requirements

| Item | Requirement |
|---|---|
| Unity | 2022.3 or later |
| Dependencies | UniTask 2.5.11 or later — nothing else |

## Installation

This package is installed from a git URL. UniTask must be resolvable first — the simplest way is a scoped registry for OpenUPM in `Packages/manifest.json`:

```jsonc
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [ "com.cysharp.unitask" ]
    }
  ]
}
```

Then, in Package Manager > `+` > *Add package from git URL...*:

```
https://github.com/Farmer0116/UILifecycle.git?path=Packages/com.farmerlab.uilifecycle#v0.1.1
```

Always pin a tag. Full instructions, including the git-URL route for UniTask, are in the [package README](Packages/com.farmerlab.uilifecycle/README.md#2-installation).

## Documentation

- [Package README](Packages/com.farmerlab.uilifecycle/README.md) — setup, vocabulary, API guide and current limitations
- [Samples](Packages/com.farmerlab.uilifecycle/Samples~/UiLifecycle) — eight numbered scenes, one feature each. Import them from the Samples tab in Package Manager and open `001` first. The samples additionally require TextMeshPro.
- [CHANGELOG](Packages/com.farmerlab.uilifecycle/CHANGELOG.md)
- [CONTRIBUTING](CONTRIBUTING.md) — how to report a bug or propose a change

## Repository layout

This repository is a Unity project that hosts the package under `Packages/`, so the samples and tests can be run directly by opening it.

```
Packages/com.farmerlab.uilifecycle/   the distributed package
├── Runtime/                          Core, EntryPoint, Provider, Registry, Transition, Components
├── Tests/Runtime/                    PlayMode tests
└── Samples~/UiLifecycle/             sample scenes 001-008
```

## Project status

Maintained by one author in their spare time, and published primarily as a
showcase of that author's work. **Pull requests are generally not accepted.**
Bug reports are welcome, though replies may be slow. If you need a change, fork
it — the MIT license allows that. See [CONTRIBUTING](CONTRIBUTING.md).

The Japanese documents are the authoritative version; the English ones are
translations.

## License
[MIT License](LICENSE.md). Copyright (c) 2026 Farmer Lab.
