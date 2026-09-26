# 006 — 演出の差し替え

[English](README.md) | **日本語**

**同じページを Presenter 違いの 4 つの Key で開き比べる。** 001〜004 の升目とも 005 の寿命とも直交する軸（演出）だけを扱う。

| Key | Presenter | 見え方 |
|---|---|---|
| `None` | 0 個 | `ImmediateTransitionPresenter` にフォールバック — 即座に出る |
| `Fade` | 1 個 | それがそのまま使われる（0.25 秒） |
| `Parallel` | 2 個 / `Parallel` | フェードと拡大が同時（0.25 秒） |
| `Sequential` | 2 個 / `Sequential` | フェード → 拡大の順（0.5 秒） |

Presenter は **EntryPoint と同じ GameObject から自動収集される**。上表の「0 / 1 / 複数」はコンポーネントの数そのもので、
Registry にも呼び側にも現れない（key は UI と 1:1 のまま）。

## 構成

| | 中身 |
|---|---|
| `FadePresenter.cs` | `CanvasGroup` の alpha を 0↔1。`UiTransitionPresenterBehaviour` を継承して 2 メソッド書くだけ |
| `ScalePresenter.cs` | `localScale` を 0.8↔1。「同じ GameObject に 2 つ付いている」状態を作るためのもう 1 つ |
| `Page_None.prefab` | Root に `UiPage` のみ、閉じるボタンに `UiCancelButton` |
| `Page_Fade.prefab` | 上に `FadePresenter` を追加（`_group` は Root の `CanvasGroup`） |
| `Page_Parallel.prefab` | さらに `ScalePresenter` を追加。`UiPage` の `Presenter Mode` = `Parallel` |
| `Page_Sequential.prefab` | `Page_Parallel` の Prefab Variant。`Presenter Mode` = `Sequential` だけ上書き |
| `UiRegistry.asset` | 上表の 4 エントリ（全て `Transient` / `Prefab`）。**Prefab 参照は空** — 4 つ作った後にインスペクタで割り当てる |
| `006_SampleScene.unity` | Key ごとに `Button_Show` / `Button_Hide` のペア 4 組 |

`Sequential` の再生順は**コンポーネントの並び順**（Fade を上に置くとフェードが先）。

## 見どころ

- **Presenter 0 個でも動く**（`None`）。演出在庫がゼロのプロダクトでも基盤はそのまま採用できる
- **合成しても `IUiTransitionPresenter`**。`UiHost` からは単体演出と区別がつかず、n の存在は EntryPoint の内側で閉じている
- **退場演出が終わるまで実体は消えない**。`Transient` の破棄は `ExitAsync` の await 後 — フェードアウト中に消えることはない
- **退場演出中に `Button_Show` を押しても無視される**。再入ゲートは `Hidden` まで開かない
- **入場演出中に `Button_Hide` を押すと、入場を最後まで再生してから退場に入る**。`RequestCancel()` は「閉じ待ち」を解くだけで、再生中の演出を切りはしない
- 入場の初期値（alpha=0 / scale=0.8）は**最初の await より前**に代入する。`PlayEnterAsync` は `SetActive(true)` の直後に呼ばれるので、これで「一瞬だけ完成形が見える」を防げる
