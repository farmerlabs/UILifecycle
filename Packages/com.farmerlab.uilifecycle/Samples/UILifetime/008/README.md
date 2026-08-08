# 008 — Custom Provider で調達を差す

**基盤に無い調達を外から差す軸。** 最小の実例として「生成先 (parent) を指定して Instantiate する」を扱う。
同じ Prefab・同じ Policy で、**違う場所に出る 2 つの key** を並べる。

| Key | Kind | Policy | CustomProviderId | 生成先 |
|---|---|---|---|---|
| `Dialog_Left` | `Custom` | `Transient` | `Anchor_Left` | `Anchor_Left`（シーン上の Transform） |
| `Dialog_Right` | `Custom` | `Transient` | `Anchor_Right` | `Anchor_Right`（同上） |

Registry の 2 エントリは `CustomProviderId` 以外まったく同じ。**生成先は Registry の外側で決まる。**

## 構成

| | 中身 |
|---|---|
| `Sample008Bootstrap.cs` | `Awake` で `RegisterProvider(id, new PrefabProvider(prefab, anchor))` を 2 つ |
| `FadePresenter.cs` | 006 と同じフェード（`CanvasGroup.alpha`） |
| `Page.prefab` | Root に `UiPage` + `CanvasGroup` + `FadePresenter`、`CloseButton` に `UiCancelButton`（ページ側のコードは 0 行） |
| `UiRegistry.asset` | 上表の 2 エントリ。**Prefab 欄は空**（Prefab 参照は Bootstrap が持つ） |
| `008_SampleScene.unity` | Canvas 下に `Anchor_Left` / `Anchor_Right`、`Button_Show_Left` / `Button_Show_Right` に `UiShowButton` |

Bootstrap の `Prefab` / `Left Anchor` / `Right Anchor` を Inspector で繋ぐこと。

## 見どころ

- **`Kind` を 1 値変えるだけで調達経路が差し替わる**（007 と同じ話の反復）。呼び側は `key` しか知らないままで、`UiShowButton` はコード 0 行のまま効く
- **Custom は「重い実装」専用ではない。** ここでは `PrefabProvider` — パッケージ標準の実装 — をそのまま `new` している。差すのは調達手段であって、依存ではない
- **生成先は Registry に置けない。** parent はシーン内の `Transform` で、Registry は SO＝アセットなのでシーン参照を持てない。Registry に欄を足す案は「シーンから自己登録する仕掛け」を `UiSceneAnchor` に続いて 2 つ目発明する話になる。差し込み口を使えば発明は要らない
- **演出は供給手段と直交する。** `FadePresenter` は 006 からそのまま持ってきていて、Custom で調達しても扱いは変わらない。`Transient` なので、退場フェードの完了を待ってから実体が破棄される
- **登録は毎回 `Awake` で。** `Host` は Registry SO が遅延生成する共有インスタンスだが、再生開始時に捨てられる（ドメインリロードを切っていても前回の登録は残らない）
- `RegisterProvider` は Show より前に済んでいればよい。`Awake` にしているのは `UiShowButton` のクリックより確実に早いから
- **Policy は `Cached` / `Transient` のみ。** `Persistent` は「基盤が調達しない」の意味なので Custom とは組み合わせられない（005 参照）
