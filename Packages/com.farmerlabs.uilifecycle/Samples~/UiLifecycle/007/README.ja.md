# 007 — Additive Scene から供給する

[English](README.md) | **日本語**

**ページを Prefab ではなくシーンから調達する。** 001〜004 の升目とも 005 の寿命とも直交する軸（供給手段）だけを扱う。

| Key | Kind | Policy | 実体 |
|---|---|---|---|
| `SceneDialog` | `AdditiveScene` | `Transient` | `007_PageScene` を Additive ロード、閉じたら Unload |

供給手段は `ProviderKind` の 1 値でしかない。**呼び側にも Bootstrap にもコードは 1 行も要らない**（001 と同じ）。

## 構成

| | 中身 |
|---|---|
| `UiRegistry.asset` | 上表の 1 エントリ。`007_PageScene` を作ったら `Scene` 欄にドロップする（`Scene Name` に自動反映） |
| `007_PageScene.unity` | Root に `UiPage`、その下に Canvas と `UiCancelButton`。**EventSystem / Camera は置かない** |
| `007_SampleScene.unity` | `Button_Show` に `UiShowButton`、`Button_Hide` に `UiHideButton`。EventSystem はこちら側だけ |

C# ファイルは無い。**両シーンを Build Settings の Scenes In Build に追加すること**（忘れると `Scene couldn't be loaded` で落ちる）。

## 見どころ

- **Prefab と扱いがまったく同じ。** 呼び側は `key` しか知らず、Registry で `Kind` を差し替えるだけで Prefab ⇄ Scene が入れ替わる。許容 Policy も Prefab と同じ（`Cached` / `Transient`）
- **`ShowAsync` が最初から `UniTask` なのはこのため。** シーンロードは原理的に同期取得できない。Prefab では同フレームで返っていた await が、ここで初めて実時間かかる
- **`Construct` は `Start` より前**（Prefab と同じ）。`AdditiveSceneProvider` が `sceneLoaded`（= `Awake` の直後・`Start` の前）で Root を落としているため。ロード直後の素の状態も 1 フレームも見えない
- **ロード中に `Button_Hide` を押しても握り潰されない。** 調達中は `EntryPoint` がまだ無いので、`UiHost` が要求を預かり `Construct` 直後に効かせる — 「入場演出中の Hide」（006）と同じ挙動になる
- **ロード自体は中断しない。** 半ロードのシーンの後始末と `ShowAsync` の戻り契約が増えるため、途中で切るのではなく上記の預かりで揃えている
- **`Persistent` は選べない**。「シーンに配置済み＝基盤は調達しない」の意味なので、常駐させたいなら起動時に自前でロードして `UiSceneAnchor`（= `SceneObject` / `Persistent`）を使う
