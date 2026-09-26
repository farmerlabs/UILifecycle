# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.1] - 2026-09-25

### Changed

- Translated the sample documentation into English. Each sample folder now has a
  `README.md` (English) alongside a `README.ja.md` (Japanese).
- Noted in the sample index that the samples additionally require TextMeshPro.

## [0.1.0] - 2026-09-25

### Added

- `UiHost` with `ShowAsync`, `ShowForResultAsync`, `HideAsync` and `IsShown`.
- Page base classes: `UiPage`, `UiEntryPoint`, `UiEntryPoint<TArgs>`, `UiEntryPointForResult<TResult>` and `UiEntryPoint<TArgs, TResult>`.
- Lifetime policies `Persistent` / `Cached` / `Transient`, combined with the acquisition kinds `SceneObject` / `Prefab` / `AdditiveScene` / `Custom`.
- `UiRegistryAsset` for mapping keys to pages, and `UiSceneAnchor` for scene-placed UI.
- Inspector components `UiShowButton`, `UiHideButton` and `UiCancelButton`.
- Pluggable transitions through `UiTransitionPresenterBehaviour`, composed in `Parallel` or `Sequential` mode.
- Custom acquisition via `UiHost.RegisterProvider`.
- Eight sample scenes and PlayMode tests.
- Documentation in English and Japanese, with the Japanese version as the authoritative one.

[Unreleased]: https://github.com/Farmer0116/UILifecycle/compare/v0.1.1...HEAD
[0.1.1]: https://github.com/Farmer0116/UILifecycle/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/Farmer0116/UILifecycle/releases/tag/v0.1.0