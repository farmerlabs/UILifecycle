# 003 — 待機あり / 引数なし / 返り値あり

**await が「閉じた後」まで伸びる最初のサンプル。** 入力フォームは「渡すものはないが返すものはある」の典型。

| 項目 | 内容 |
|---|---|
| 呼び側 | `Sample003Bootstrap` → `ShowForResultAsync<string>(key)` |
| 閉じ方 | 決定 = `Close(_input.text)`（コード） / キャンセル = `UiCancelButton`（子）・`UiHideButton`（親） |
| ページ基底 | `UiEntryPointForResult<string>`（= `<Unit, string>`。`Unit` は書かない） |
| await が返る時点 | **相手が閉じた後**（結果つき） |

`ShowAsync` との差は await が返る時点で、名前に出してある
（Android の `startActivity` / `startActivityForResult` と同じ分離）。
結果の受取口は **await の戻り値だけ** — だから「返り値あり × 待たない」は原理的に存在しない。

## 構成

| | 中身 |
|---|---|
| `NameInputPage.cs` | `OnShow` で入力欄を初期化し、決定ボタンだけ購読（`Close(_input.text)`） |
| `Page.prefab` | Root に `NameInputPage`、`Button_Ok` を `_decideButton` に割り当て、`Button_Cancel` に `UiCancelButton` |
| `UiRegistry.asset` | Key = `NameInput` / Policy = `Transient` / Kind = `Prefab` |
| `003_SampleScene.unity` | `Button_Show` + `Sample003Bootstrap`（結果表示の TMP_Text つき）、`Button_Hide(Cancel)` に `UiHideButton` |

## 見どころ

- **確定を作れるのは子だけ**: `HasValue = true` ⇔ ページが `Close(result)` を呼んだ。決定側だけコードを書くのはそのため
- **キャンセル経路は全部同じ形で返る**: `UiCancelButton`（子）/ `UiHideButton`（親）/ 再入無視、どれも `HasValue = false`。呼び側の分岐は `if (result.HasValue)` 1 個で済む
- 入力中に `Button_Hide(Cancel)` を押すと、**待っている Bootstrap の await が `HasValue = false` で返る**のを実際に確かめられる
- `await` が永遠に返らない状態は不変条件として禁止されている
