# UiLifecycle サンプル

**番号順に読むと、1 要素ずつ足されていく。** 軸の直積ではなく読む順で並べてある。

| # | 待機 | 引数 | 返り値 | 呼び側 API | ページ基底 |
|---|---|---|---|---|---|
| [001](001/) | なし | なし | なし | `ShowAsync(key)` | `UiPage`（コード 0 行） |
| [002](002/) | なし | あり | なし | `ShowAsync(key, args)` | `UiEntryPoint<TArgs>` |
| [003](003/) | あり | なし | あり | `ShowForResultAsync<TResult>(key)` | `UiEntryPointForResult<TResult>` |
| [004](004/) | あり | あり | あり | `ShowForResultAsync<TArgs, TResult>(key, args)` | `UiEntryPoint<TArgs, TResult>` |

005 以降は**この升目と直交する軸**を 1 つずつ扱う（1 サンプル 1 主題）。
どれも 001〜004 のどの象限とも組み合わせられる。

| # | 軸 | 比べるもの |
|---|---|---|
| [005](005/) | 寿命 | `LifetimePolicy` = `Transient` / `Cached` / `Persistent` |
| [006](006/) | 演出 | Presenter が 0 個 / 1 個 / 複数（`Parallel` / `Sequential`） |
| [007](007/) | 供給手段 | `ProviderKind` = `AdditiveScene`（Prefab との差は Registry の 1 値だけ） |
| [008](008/) | 供給手段 | `ProviderKind` = `Custom`（調達を外から差す。例として生成先を指定） |

## なぜ 4 通りなのか（8 でも 6 でもなく）

軸は 3 つ（待機 / 引数 / 返り値）に見えるが、**待機と返り値は独立していない**。
結果の受取口が `await` の戻り値しかないので:

- 「**返り値あり × 待たない**」は原理的に存在しない（受け取る場所がない）
- 「**返り値なし × 待つ**」は待つ動機がない。「閉じるまで待ちたいが結果は要らない」場合は
  003 / 004 で受けて `HasValue` だけ見ればよい（キャンセル系は全経路 `HasValue=false` に統一済み）

残る自由度は **引数の有無 × 待つか** の 2×2 = 4。
これは `IUiHost` の 4 メソッドとも、ページ基底の 4 象限とも一致する。

## 共通の作法

- **`Unit` はどこにも現れない**。呼び側 4 メソッド / ページ基底 4 種で全象限が覆われている
- `registry.Host` が Registry SO の持つ共有 `UiHost`（遅延生成）。
  自前で `new UiHost(registry)` してもよいが、再入ゲートは Host 単位なので同一 Registry で混在させない
- 各サンプルは**自己完結**（専用の Scene / Registry.asset / Prefab を持つ）
- `OnShow(args)` は**表示のたび**に呼ばれる。Policy を変えても挙動は変わらない（差はメモリと調達コストだけ）

## シーン / Prefab / Registry の作り方

各フォルダの README に手順がある。C# 以外（`.unity` / `.prefab` / `.asset`）は
Unity エディタ上で作成すること。
