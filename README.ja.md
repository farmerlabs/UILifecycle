# UI Lifecycle for Unity

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity)](https://unity.com/)
[![Release](https://img.shields.io/github/v/tag/Farmer0116/UILifecycle?label=release&sort=semver)](https://github.com/Farmer0116/UILifecycle/releases)

[English](README.md) | **日本語**

UI の存在状態（Hidden / Showing / Shown / Hiding）を標準化し、遷移点に「完了を返せる何か」を差せるようにする基盤。

実体は **Unity のコンポーネントに欠けているコンストラクタを、UI の生成に取り戻すこと**。

```csharp
// ページを開く → 閉じるまで待つ → 結果が戻り値で届く
var result = await host.ShowForResultAsync<ItemDetailArgs, string>(
    "ItemDetail", new ItemDetailArgs(itemId));

if (result.HasValue)
    Debug.Log($"決定: {result.Value}");
```

## 特徴

- **4 つの相を 1 つの語彙で。** どの経路で閉じても `Showing → Shown → Hidden` の遷移は同一。
- **引数と結果に型がつく。** ページが受け取るもの・返すものを宣言し、呼び側は戻り値で結果を受け取る。イベント配線は不要。
- **待つかどうかは呼び側が選ぶ。** `ShowAsync` は Shown で返り、`ShowForResultAsync` は閉じて解放され切ってから返る。
- **単純な UI はコード 0 行。** `UiPage` と 3 つの Inspector コンポーネントだけで開閉が成立する。
- **演出は差し替え可能。** 契約は「`UniTask` で完了を返す」だけ。Feel / DOTween / USS / 自前、何でもよい。
- **寿命は宣言で決まる。** `Persistent` / `Cached` / `Transient` × `SceneObject` / `Prefab` / `AdditiveScene` / `Custom`。

## 動作要件

| 項目 | 要件 |
|---|---|
| Unity | 2022.3 以降 |
| 依存 | UniTask 2.5.11 以上のみ |

## インストール

本パッケージは git URL から導入します。先に UniTask が解決できる必要があり、最も簡単なのは `Packages/manifest.json` に OpenUPM のスコープドレジストリを足す方法です。

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

そのうえで Package Manager > `+` > *Add package from git URL...*:

```
https://github.com/Farmer0116/UILifecycle.git?path=Packages/com.farmerlab.uilifecycle#v0.1.1
```

タグは必ず明示してください。UniTask を git URL で入れる手順を含む詳細は [パッケージ README](Packages/com.farmerlab.uilifecycle/README.ja.md#2-インストール) にあります。

## ドキュメント

- [パッケージ README](Packages/com.farmerlab.uilifecycle/README.ja.md) — 導入手順、語彙の規則、API ガイド、現時点の制約
- [サンプル](Packages/com.farmerlab.uilifecycle/Samples~/UiLifecycle) — 機能ごとに 1 つずつ、8 本の番号付きシーン。Package Manager の Samples タブからインポートし、まず `001` を開いてください。サンプルの実行には別途 TextMeshPro が必要です。
- [CHANGELOG](Packages/com.farmerlab.uilifecycle/CHANGELOG.md)
- [コントリビューションガイド](CONTRIBUTING.ja.md) — バグ報告と変更提案の方法

## リポジトリ構成

このリポジトリは `Packages/` 配下にパッケージを置いた Unity プロジェクトです。そのまま開けばサンプルとテストを実行できます。

```
Packages/com.farmerlab.uilifecycle/   配布されるパッケージ
├── Runtime/                          Core, EntryPoint, Provider, Registry, Transition, Components
├── Tests/Runtime/                    PlayMode テスト
└── Samples~/UiLifecycle/             サンプルシーン 001-008
```

## 運営方針

作者ひとりが空き時間で保守しており、公開の主目的は作者自身の成果物の公開です。**プルリクエストは原則受け付けていません。** バグ報告は歓迎しますが、返信は遅いことがあります。変更が必要なら fork してください（MIT ライセンスはそれを許しています）。詳細は [コントリビューションガイド](CONTRIBUTING.ja.md) を参照してください。

日本語版のドキュメントが正本で、英語版はその翻訳です。

## ライセンス
[MIT License](LICENSE.md). Copyright (c) 2026 Farmer Lab.
