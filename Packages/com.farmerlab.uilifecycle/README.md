# Farmer Lab UI Lifecycle — 導入・使用手順

UI の存在状態（Hidden / Showing / Shown / Hiding）を標準化し、遷移点に「完了を返せる何か」を差せるようにする基盤。
実体は **Unity のコンポーネントに欠けているコンストラクタを、UI の生成に取り戻すこと**。

---

## 1. 動作要件

| 項目 | 要件 |
|---|---|
| Unity | 2022.3 以降 |
| 依存 | **UniTask 2.5.11 以上のみ**

## 2. インストール

> **本リポジトリは private です。** 事前に作者から招待を受け、かつ Git の認証設定が済んでいる必要があります。
> 認証が未設定だと、URL を入力しても取得に失敗します（→ [2-1](#2-1-git-認証を確認未設定の場合)）。

順序が重要です。**2-1 → 2-2 → 2-3 の順に実施してください。**

### 2-1. Git 認証を確認（未設定の場合）

Unity Package Manager は内部で `git` コマンドを呼ぶため、Unity ではなく **OS 側の Git が GitHub に認証できる状態**である必要があります。

ターミナルで以下が成功すれば準備完了です。

```
git ls-remote https://github.com/Farmer0116/UILifecycle.git
```

失敗する場合は、次のいずれかを設定してください。

| 方式 | 設定内容 |
|---|---|
| HTTPS | [Git Credential Manager](https://github.com/git-ecosystem/git-credential-manager) を導入し、GitHub にサインイン |
| SSH | GitHub に SSH 公開鍵を登録（この場合、後述の URL を `ssh://git@github.com/Farmer0116/UILifecycle.git?path=...` に置き換える） |

### 2-2. UniTask を用意（必須）

本パッケージは **UniTask 2.5.11 以上**に依存します。以下の A / B いずれかを選んでください。

#### 2-2-A. OpenUPM 経由（推奨）

プロジェクトの `Packages/manifest.json` に、スコープドレジストリを追記します。

```jsonc
{
  "dependencies": { /* 既存のまま */ },
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [ "com.cysharp.unitask" ]
    }
  ]
}
```

これだけで完了です。UniTask は本パッケージの `dependencies` により **2-3 で自動的に導入**されます。手動でのインストールは不要です。

#### 2-2-B. git URL 経由（代替）

OpenUPM を使えない場合はこちら。ただし次の 2 点を守る必要があります。

- **バージョンは 2.5.11 以上**（本パッケージが要求する下限）
- **必ず本パッケージ（2-3）より先に導入する**

Package Manager > `+` > *Add package from git URL...*

```
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

> 順序を誤って本パッケージを先に入れると、UPM が UniTask をレジストリから探しに行き、解決に失敗します。

### 2-3. 本パッケージを導入

Package Manager > `+` > *Add package from git URL...*

```
https://github.com/Farmer0116/UILifecycle.git?path=Packages/com.farmerlab.uilifecycle#v0.1.0
```

`?path=` はリポジトリ内のパッケージ位置、`#v0.1.0` は取得するタグです。
タグを省略するとデフォルトブランチの最新が取得され、予告なく変わります。**試用時はタグを明示してください。**

2-2-A を選んだ場合は、この時点で UniTask も自動的に取得されます。

Unity を開いてコンパイルが通れば導入完了。

### 2-4. サンプルをインポート（任意）

Package Manager > 本パッケージを選択 > **Samples** タブ > *Sample UI Lifecycle* の **Import**

以下に展開されます。

```
Assets/Samples/Farmer Lab UI Lifecycle/0.1.0/Sample UI Lifecycle/
```

001〜008 の番号付きフォルダに、機能ごとのシーンと解説 README が入っています。まず `001` のシーンを開いて再生するのが最短です。

---

## 3. 語彙の規則

このパッケージの API 名は、次の 4 行だけで読める。

> - **Show / Hide** は**親**（key しか知らない層）が Host に出す命令。
> - **Close / Cancel** は**子**（ページ自身）が出す閉じ要求。Close は確定、Cancel は取り消し。
> - **待つ / 待たない** は `ShowForResultAsync` と `ShowAsync` の差だけが表現する。ページの型も閉じ側の語彙も関与しない。
> - **確定を作れるのは子だけ**。親は `TResult` を知らないので、親から閉じれば必ずキャンセルになる。

| 語 | 層 | 主語 | 実体 |
|---|---|---|---|
| Show / Hide | Host の外部 API | key しか知らない呼び側（親） | `ShowAsync`, `ShowForResultAsync`, `HideAsync`, `IsShown` |
| Close / Cancel | ページ内部 | ページ自身（子） | `Close(result)`, `Cancel()` |

相（`UiPhase`）も親側の語彙に揃えてある: `Showing → Shown → Hiding → Hidden`。
どの経路で閉じても相の遷移は同一なので、Close / Cancel は相の名前には出ない。

## 4. 全体像 — 使い分けに必要な最小の前提知識

### 4-1. ページの種類 — 軸は「引数」と「結果」の 2 つだけ

| 基底クラス | 引数 | 結果 | 子側の閉じ方 |
|---|:-:|:-:|---|
| `UiPage` | – | – | なし（コード 0 行。閉じるのは外部かボタン） |
| `UiEntryPoint` | – | – | `Close()` / `Cancel()` |
| `UiEntryPoint<TArgs>` | ○ | – | `Close()` / `Cancel()` |
| `UiEntryPointForResult<TResult>` | – | ○ | `Close(result)` / `Cancel()` |
| `UiEntryPoint<TArgs, TResult>` | ○ | ○ | `Close(result)` / `Cancel()` |

### 4-2. 「待つ / 待たない」はページの種類ではない — 直交する

**「投げっぱなし」の API は存在しない**。あるのは「どこまで待つか」の 2 種:

| API | await が返る時点 |
|---|---|
| `ShowAsync` | **Shown**（入場演出の完了。閉じ〜解放は基盤が裏で回す） |
| `ShowForResultAsync` | **Closed**（退場演出と解放の完了後。結果つき） |

待つ / 待たないは**呼び側が 1 回ごとに選ぶ**もので、ページの型は関与しない:

- 結果ありページを `ShowAsync` で開いてよい（結果は捨てられる）
- 結果なしページを `ShowForResultAsync` で開いてよい（「閉じたこと」だけ待てる）

含意は片方向: 結果を受け取る ⇒ 必ず閉じるまで待つ。逆は成り立たない。

### 4-3. 閉じる経路は 4 つ、観測できる結果は 2 つ

| 経路 | 主体 | 確定 / キャンセル | `UiResult` |
|---|---|---|---|
| `Close(result)` | 子 | 確定 | `HasValue = true` |
| `Cancel()` | 子 | キャンセル | false |
| `HideAsync(key)` | 親 | キャンセル | false |
| 再入無視（表示中に同一 key を再 Show） | 親 | キャンセル | false |

`HasValue = true` ⇔ **子が `Close(result)` を呼んだ**、が一意に成立する。
逆に **`HasValue = false` の原因は 3 つある**（子の Cancel / 親の Hide / 再入無視）が、呼び側からは区別できない — 区別する軸を作らないのは設計判断。

再入無視でキャンセルされるのは**既に開いているページではなく、2 回目の呼び出しのほう**。既存の表示には触れない。ゲートは閉じ切る（解放完了）まで残る。

### 4-4. Component ボタン — 成立するのは 3 つだけ

「開く / 閉じる × 親 / 子」で升目を作ると、`[SerializeField]` だけで表現できるのは 3 つ。残りは構造的に空（型付きデータが流れた時点でコードを書く層に上がる）。

| コンポーネント | 対応 API | 置き場所 | 必要な設定 |
|---|---|---|---|
| `UiShowButton` | `ShowAsync(key)` | 呼び側（親） | Registry + Key |
| `UiHideButton` | `HideAsync(key)` | 呼び側（親） | Registry + Key |
| `UiCancelButton` | ページ自身の `Cancel` 相当 | Page の Root 配下（子） | なし |

空いている升目とその理由:

- 開く / 引数あり・待機あり → 型付き引数・結果を Inspector に置けない
- 閉じる / 親 / **確定** → 親は `TResult` を知らない。**原理的に不可能**（§3 の 4 行目）
- 閉じる / 子 / **確定** → 結果は子が自分の状態から組み立てるもの。Inspector では作れない

`UiCancelButton` が「Close」ではなく「Cancel」を名乗るのは、このボタンが確定を作れないから。

---

## 5. クイックスタート・層1（コード 0 行）

データが流れない UI（設定画面を開く、ポップアップを閉じる等）は、コードを 1 行も書かずに動く。

### 5-1. ページを作る

Prefab の Root に **`UiPage`** を付けるだけ（Canvas 配下でも 3D でも可。基盤は見た目を知らない）。

### 5-2. Registry を作る

Project ビューで右クリック > **Create > UiLifecycle > Registry**。

インスペクタでエントリを追加:

| フィールド | 値 |
|---|---|
| Key | `Settings`（名詞 = UI の ID） |
| Policy | `Transient`（表示ごと生成・破棄） |
| Kind | `Prefab` |
| Prefab | 5-1 の Prefab |

### 5-3. ボタンを配線する

- 開く側: 任意の `Button` に **`UiShowButton`** を付け、Registry と Key を Inspector で設定
- 閉じる側（子）: ページ内の `Button` に **`UiCancelButton`** を付けるだけ（Key 不要 — 親のページを自動で拾う）
- 閉じる側（親）: ページの外の `Button` に **`UiHideButton`** を付け、Registry と Key を設定（対象が開いていなければ何も起きない）

これで開閉・演出待ち・寿命管理・再入ゲートが全部効いた状態で動く。

コードから開きたい場合も引数なし 1 行:

```csharp
await registry.Host.ShowAsync("Settings");   // 入場演出完了 (Shown) で返る
```

`registry.Host` は Registry SO が持つ共有 Host（遅延生成・ドメインリロードでリセット）。
自前で `new UiHost(registry)` してもよいが、再入ゲートは Host 単位なので同一 Registry で併用しないこと。

---

## 6. クイックスタート・層2（データが流れる時だけ書く）

境界は「**型付きデータが流れるか**」。流れるなら、そのページの型を書く — それが「コンストラクタを取り戻す」の実体。

### 6-1. ページを書く

継承するのは §4-1 の表から選ぶ。書くのは `OnShow` と、閉じたいときの `Close` / `Cancel`。

```csharp
using UiLifecycle;
using UnityEngine;

public readonly struct ItemDetailArgs
{
    public readonly int ItemId;
    public ItemDetailArgs(int itemId) => ItemId = itemId;
}

// TArgs = 開くときに受け取るもの / TResult = 閉じるときに返すもの
public sealed class ItemDetailPage : UiEntryPoint<ItemDetailArgs, string>
{
    // 表示のたびに呼ばれる。ここで毎回組み立て直す = 「再表示 = 初期状態」
    protected override void OnShow(ItemDetailArgs args)
    {
        // args.ItemId から画面を構築
    }

    // ボタン等から呼ぶ
    public void OnDecideButton() => Close("decided");   // 確定して閉じる
    public void OnBackButton()   => Cancel();           // 取り消して閉じる
}
```

Registry への登録は層1 と同じ。

**どの基底を選んでも `Unit` を書く機会はない**。型引数は末尾から省略すると埋まる（`UiEntryPoint<TArgs>` = `<TArgs, Unit>`）。
「引数なし × 結果あり」だけは先頭を省く形になり末尾省略に乗らないため、別名 `UiEntryPointForResult<TResult>` で公開している（呼び側の `ShowForResultAsync<TResult>(key)` と対応）。

### 6-2. 開いて、結果を受け取る

await が返る時点が名前に出ている（Android の `startActivity` / `startActivityForResult` と同じ分離。§4-2）:

```csharp
using UiLifecycle;
using Cysharp.Threading.Tasks;

public sealed class Bootstrap : MonoBehaviour
{
    [SerializeField] private UiRegistryAsset _registry;
    private UiHost _host;

    private void Awake() => _host = new UiHost(_registry);

    public async UniTaskVoid OpenDetail(int itemId)
    {
        // 開く → 相手が閉じるまで待つ → 結果が戻り値で届く（イベント配線ゼロ）
        var result = await _host.ShowForResultAsync<ItemDetailArgs, string>(
            "ItemDetail", new ItemDetailArgs(itemId));

        if (result.HasValue)
            Debug.Log($"決定: {result.Value}");   // 子が Close(result) を呼んだ
        else
            Debug.Log("キャンセルされた");         // 子の Cancel / 親の Hide / 再入無視
    }
}
```

`UiHost` は純 C# クラス。公開方法（DI / シングルトン / 直接 new / `registry.Host`）は使用者のアーキテクチャに委ねる。

結果が要らないが引数は渡したい場合:

```csharp
await _host.ShowAsync("ItemDetail", new ItemDetailArgs(itemId));
// ↑ Shown で await が返る。閉じ〜解放は基盤が裏で処理
```

引数は不要だが結果は欲しい場合（入力フィールドを動的生成して入力値を受け取る等）:

```csharp
var result = await _host.ShowForResultAsync<string>("InputForm");
// ページ側は UiEntryPointForResult<string> を継承し Close(text) で返す
```

親から閉じる（必ずキャンセルになる。§4-3）:

```csharp
await _host.HideAsync("ItemDetail");  // 退場演出〜解放の完了まで待つ
```

---

## 7. シーン配置 UI（Persistent）

シーンに直接置いた UI も同じ `ShowAsync` / `HideAsync` で扱う。

1. シーン上の GameObject に `UiPage`（または `UiEntryPoint` 継承クラス）を付ける
2. 同じ GameObject に **`UiSceneAnchor`** を追加し、Registry と Key を設定
3. Registry 側は Key / `Policy = Persistent` / `Kind = SceneObject`（Prefab 不要）

`UiSceneAnchor` が Awake で Registry へ自己登録する。`_hideOnAwake`（既定 ON）で登録後に非表示化される。

> 注意: **非アクティブな GameObject は Awake が走らず登録されない**。初期非表示にしたい場合はアクティブのまま `_hideOnAwake` を使う。

## 8. Policy × Kind の組み合わせ

| Policy | 意味 | 許される Kind |
|---|---|---|
| `Persistent` | シーン常駐。基盤は生成/破棄しない | `SceneObject` |
| `Cached` | 初回に調達し、以後再利用（閉=非表示） | `Prefab` / `AdditiveScene` / `Custom` |
| `Transient` | 表示ごと調達、閉じたら解放 | `Prefab` / `AdditiveScene` / `Custom` |

不正な組み合わせは実行時に理由つき例外で弾かれる（静的×破壊は表現不能）。

**`Persistent` が `SceneObject` 専用なのは、Policy が「基盤が調達するか」も決めているから。**
「シーンに配置済み＝調達しない」の意味なので、調達する Kind と組み合わせると指定が自己矛盾する。
塞いでも表現力は落ちない — 常駐させたいなら `SceneObject` + `Persistent`、遅延常駐なら `Cached` が同じ挙動を覆う。

どの Policy でも **表示のたびに `OnShow(args)` が呼ばれる**。挙動差は寿命だけ。

### 8-1. Kind — 調達の手段（Policy とは直交）

| Kind | 調達 | 備考 |
|---|---|---|
| `SceneObject` | しない（`UiSceneAnchor` が自己登録） | `Persistent` 専用 |
| `Prefab` | `Instantiate` | 同期完了する |
| `AdditiveScene` | シーンを Additive ロード → Root から `IUiEntryPoint` を発見 | `Scene` にシーンをアタッチ。**別途 Build Settings への登録が必要** |
| `Custom` | `UiHost.RegisterProvider(id, provider)` で差した実装 | Addressables 等、外部依存が要る調達はここ |

`Scene` はインスペクタにシーンアセットをドロップする欄で、名前が実行時用の `Scene Name` に自動反映される
（`SceneAsset` は Editor 専用型でビルドに残せないため、実行時の真実は常に `Scene Name`）。
アタッチはリネーム追従と typo 防止のためのもので、**Build Settings 登録の代わりにはならない** — Unity の制約。

`AdditiveScene` の運用上の制約（`Prefab` には無いもの）:

- UI シーンに **EventSystem / Camera を置かない**（重複で入力が壊れる）
- Unload はシーン所属の GameObject しか消さない。**UI シーンから他シーンへ生成物を漏らさない**
- **1 シーン 1 ページ**。同じシーン名を複数の key に割り当てない

## 9. 演出を差す（任意）

基盤の契約は「`UniTask` で完了を返す」だけ。中身は Feel / DOTween / USS / 自前、何でもよい。

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using UiLifecycle;

public sealed class FadePresenter : UiTransitionPresenterBehaviour
{
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private float _duration = 0.2f;

    public override async UniTask PlayEnterAsync(CancellationToken ct)
    {
        for (var t = 0f; t < _duration; t += Time.deltaTime)
        {
            _group.alpha = t / _duration;
            await UniTask.Yield(ct);
        }
        _group.alpha = 1f;
    }

    public override async UniTask PlayExitAsync(CancellationToken ct)
    {
        for (var t = 0f; t < _duration; t += Time.deltaTime)
        {
            _group.alpha = 1f - t / _duration;
            await UniTask.Yield(ct);
        }
        _group.alpha = 0f;
    }
}
```

EntryPoint と同じ GameObject に付けるだけで自動収集される:

- 0 個 → 即完了（演出在庫ゼロでも動く）
- 1 個 → それを使用
- 複数 → 合成。EntryPoint インスペクタの `Presenter Mode` で `Parallel` / `Sequential` を選択

**収集するのは EntryPoint と同じ GameObject だけ**（`GetComponents`。子は見ない）。子に付けても無言で無視される。
動かす対象は `SerializeField` の参照先なので子でよく、「**コンポーネントは Root、対象は各部位**」が基本形。
`Sequential` の順序は Root 上の並び順で決まる（レイアウト都合で階層を組み替えても再生順は変わらない）。

退場演出の完了を await してから解放するので、フェードアウト中に実体が消えることはない。

## 10. 知っておく仕様

- **Construct は Start より前**: 供給手段によらず成立する。「Start 以降は引数が入っている」と覚えればよい。購読側は `OnConstructed`（発火済みなら購読時に即再生）で取りこぼさない
  - 成立の根拠は `IUiInstanceProvider` の契約「**調達直後の実体は非表示**」。`Prefab` は `Instantiate` が同期完了するため自動的に満たし、フレームを跨ぐ調達（`AdditiveScene` / Addressables）は明示的に落とすことで満たす。自前 Provider を書くときはここを守ること（守らないと `Start` が追い越し、入場演出の初期値が乗る前に素の状態が見える）
- **調達中の Hide も効く**: 実体がまだ無い間の `HideAsync` は `UiHost` が預かり、`Construct` 直後に反映する。「入場演出中の Hide」と同じく、開き切ってから閉じる挙動になる。ロード自体は中断しない
- **再入は無視**: 同一 key の表示中に再度 Show しても 2 枚目は開かない。ただし await は**必ず返る**（即 `HasValue=false`。§4-3）
- 同一 key の同時 2 枚表示は非対応（key = UI の ID）
- **`Unit` を書く機会はない**: 呼び側は 4 メソッド（`ShowAsync` / `ShowForResultAsync` × 引数あり/なし）、ページ側は 4 基底（§4-1 の表）で全象限が覆われている。`Unit` は実装詳細
- **ページの `TResult` は「能力」であって義務ではない**: `ShowAsync<TArgs>` は結果を要求しないので、`UiEntryPoint<TArgs, TResult>` なページを結果を捨てて開いてよい。結果を受け取るかは呼び側の選択（§4-2）

## 11. テスト実行

**Window > General > Test Runner > PlayMode** タブ → `UiLifecycle.Tests` → Run All（11 本）。

テスト assembly は `UNITY_INCLUDE_TESTS` 制約付きなので、ビルドには含まれない。

## 12. 現時点の制約

- `CancellationToken` キャンセル時の途中巻き戻しは最小限。**調達（シーンロード等）は途中で中断しない** — 半ロードの後始末と `ShowAsync` の戻り契約が増えるため、閉じ要求は §10 の預かりで揃えている
- `Kind = Custom`（Addressables 等、外部依存が要る調達）は差し込み口 `UiHost.RegisterProvider(id, provider)` のみ提供。Provider 実装は製品側
- key 定数の自動生成（UiKeys.g.cs + `[UiKey]` ドロップダウン）は未実装。当面は文字列直書き
