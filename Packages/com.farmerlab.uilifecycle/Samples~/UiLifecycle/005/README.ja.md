# 005 — LifetimePolicy の使い分け

[English](README.md) | **日本語**

**同じページを Policy 違いの 3 つの Key で開き比べる。** 001〜004 と直交する軸（寿命）だけを扱う。

| Key | Policy | 実体 |
|---|---|---|
| `Transient` | 表示ごとに生成、閉じたら破棄 | Prefab |
| `Cached` | 初回だけ生成、以後再利用（閉 = 非表示） | Prefab |
| `Persistent` | シーン常駐。基盤は生成/破棄しない | シーン上の GameObject + `UiSceneAnchor` |

## 構成

| | 中身 |
|---|---|
| `PolicyProbePage.cs` | 「instance #n / shown xN」を表示するだけ（n = Awake 起点、N = OnShow 起点） |
| `Page.prefab` | Root に `PolicyProbePage`、閉じるボタンに `UiCancelButton` |
| `UiRegistry.asset` | 上表の 3 エントリ |
| `005_SampleScene.unity` | Key ごとに `UiShowButton` / `UiHideButton` のペア 3 組。Persistent 用ページはシーンに直接置き `UiSceneAnchor` で登録 |

## 見どころ

- **`Transient`**: 開くたび `instance #` が進み、`shown` は常に x1 — 毎回作り直されている
- **`Cached` / `Persistent`**: `instance #` は固定で `shown` が進む — 実体が残っている
- **どれも `OnShow` は毎回走る**（挙動は同一、差はメモリと調達コストだけ）。`Cached` でも表示内容が古くならないのはこのため
- `UiSceneAnchor` は Awake で自己登録する。**非アクティブだと Awake が走らず登録されない**ので、初期非表示は `_hideOnAwake`（既定 ON）で行う
