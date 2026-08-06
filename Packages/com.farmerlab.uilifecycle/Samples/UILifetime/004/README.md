# 004 — 待機あり / 引数あり / 返り値あり

**全部入り。** 003 に引数が乗っただけで、await の性質は変わらない。

| 項目 | 内容 |
|---|---|
| 呼び側 | `Sample004Bootstrap` → `ShowForResultAsync<ItemDetailArgs, string>(key, args)` |
| 閉じ方 | 確定 = `Close("decided")`（コード） / キャンセル = `UiCancelButton`（子）・コードの `HideAsync`（親） |
| ページ基底 | `UiEntryPoint<ItemDetailArgs, string>` |
| await が返る時点 | **相手が閉じた後**（結果つき） |

開閉は「対称」ではなく **in / out**。下りが `args`（呼び側 → UI）、上りが `result`（UI → 開いた本人）。
上りの受け手が「開いた本人」に決まっているから、結果は戻り値で足りる。

## 構成

| | 中身 |
|---|---|
| `ItemDetailArgs.cs` | 表示引数（readonly struct） |
| `ItemDetailPage.cs` | `OnShow(args)` で表示を組み立て、決定ボタンだけ購読（`Close("decided")`） |
| `Page.prefab` | Root に `ItemDetailPage`、`Button_Close` を `_closeButton` に割り当て、`Button_Cancel` に `UiCancelButton` |
| `UiRegistry.asset` | Key = `ItemDetail` / Policy = `Transient` / Kind = `Prefab` |
| `004_SampleScene.unity` | `Button_Show` / `Button_Hide` + `Sample004Bootstrap`（結果表示の TMP_Text つき） |

## 見どころ

- 決定 → `Result: decided`、キャンセル → `(canceled)`。呼び側の分岐は `if (result.HasValue)` 1 個
- **`Button_Hide` は 001〜003 の `UiHideButton` のコード版**（`HideAsync(key)` を Bootstrap から呼ぶ）。待っている本人の await が `HasValue = false` で返る — 親から閉じれば必ずキャンセル
- Policy を `Transient` → `Cached` に変えても**挙動は変わらない**（どちらも毎回 `OnShow(args)`）。差はメモリと調達コストだけ。使い分けは 005 を参照
