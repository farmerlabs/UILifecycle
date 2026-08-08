# 001 — 待機なし / 引数なし / 返り値なし

**コード 0 行。** 設定画面・クレジット・ヘルプなど、型付きデータが流れない UI はここで足りる。

| 項目 | 内容 |
|---|---|
| 呼び側 | `UiShowButton`（= `ShowAsync(key)`）/ `UiHideButton`（= `HideAsync(key)`） |
| 閉じ方 | 子: `UiCancelButton`（ページ内） / 親: `UiHideButton`（シーン側） |
| ページ基底 | `UiPage` |
| await が返る時点 | 入場演出完了 (Shown)。閉じるのは待たない |

## 構成

| | 中身 |
|---|---|
| `Page.prefab` | Root に `UiPage`、`CloseButton` に `UiCancelButton`（Key 不要 — 親のページを自動で拾う） |
| `UiRegistry.asset` | Key = `Dialog` / Policy = `Transient` / Kind = `Prefab` / Prefab = `Page.prefab` |
| `001_SampleScene.unity` | `Button_Show` に `UiShowButton`、`Button_Hide` に `UiHideButton`（どちらも Registry + Key = `Dialog`） |

## 見どころ

- **スクリプトが 1 つも無い。** 開閉・演出待ち・寿命管理・再入ゲートが全部効いている
- **コンポーネント 3 種が全部揃う**: 開く = `UiShowButton`（親）/ 閉じる = `UiHideButton`（親）と `UiCancelButton`（子）。成立する升目はこの 3 つだけ（パッケージ README §4-4）
- `UiHideButton` は対象が開いていなければ**黙って no-op**（押しても何も起きないのが正常系）
- `Transient` なので閉じると実体は破棄される。開き直すと作り直される
- 結果を返す閉じ方は 003 / 004 を参照

コードから開く形は 002 以降で扱う。
