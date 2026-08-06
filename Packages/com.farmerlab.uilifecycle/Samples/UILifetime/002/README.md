# 002 — 待機なし / 引数あり / 返り値なし

**001 に「引数」だけ足した形。** 型付きデータが流れるので、初めてページクラスを書く。
ただし書くのは流れる部分（`args.Message`）だけ — 閉じは 001 と同じ `UiCancelButton`（コード 0 行）。

| 項目 | 内容 |
|---|---|
| 呼び側 | `Sample002Bootstrap` → `ShowAsync(key, args)` |
| 閉じ方 | 子: `UiCancelButton`（ページ内） / 親: `UiHideButton`（シーン側） |
| ページ基底 | `UiEntryPoint<ToastArgs>`（= `<ToastArgs, Unit>`。`Unit` は書かない） |
| await が返る時点 | 入場演出完了 (Shown)。001 と同じ |

## 構成

| | 中身 |
|---|---|
| `ToastArgs.cs` | 表示引数（readonly struct） |
| `ToastPage.cs` | `OnShow(args)` でメッセージを組み立てるだけ |
| `Page.prefab` | Root に `ToastPage`、OK ボタンに `UiCancelButton` |
| `UiRegistry.asset` | Key = `Toast` / Policy = `Transient` / Kind = `Prefab` |
| `002_SampleScene.unity` | `Button_Show` + `Sample002Bootstrap`（開くたびに連番付きの文言を渡す）、`Button_Hide` に `UiHideButton` |

## 見どころ

- **引数は「開くたび」に渡す**: 生成時 1 回ではないので、2 回目以降も `OnShow(args)` が走り毎回の文言で組み立て直される
- 開くボタンを連打しても**表示中の再入は無視**される（`await` は必ず返るので呼び側は固まらない）
- 開くのはコード（引数が要る）、閉じるのはコンポーネントで足りる（データが流れない）— 層の境界が開閉で非対称になる例
- 結果を返す閉じ方は 003 / 004 を参照。Policy の使い分けは 005 を参照
