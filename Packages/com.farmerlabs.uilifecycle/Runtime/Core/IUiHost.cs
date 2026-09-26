using System.Threading;
using Cysharp.Threading.Tasks;

namespace UiLifecycle
{
    /// <summary>
    /// 使用者が触る唯一の入口。呼び側が知るのは key と args だけ。
    /// 静的/動的・Prefab/Scene・寿命は Registry に隠れ、呼び側に漏れない。
    /// ShowAsync が「必ず」非同期なのは、Additive Scene / Addressables が
    /// 原理的に同期取得できないため (後からシグネチャを変えると全呼び側が壊れる)。
    /// </summary>
    public interface IUiHost
    {
        /// <summary>
        /// 表示し、閉じるまで待って結果を受け取る。await が返るのは「相手が閉じた後」。
        /// UI 側の Close(result) で HasValue=true、Cancel/Hide/再入無視 で HasValue=false。
        /// 命名は startActivity / startActivityForResult と同じ分離
        /// (await が返る時点の差を、ジェネリック引数でなく名前で示す)。
        /// </summary>
        UniTask<UiResult<TResult>> ShowForResultAsync<TArgs, TResult>(string key, TArgs args, CancellationToken ct = default);

        /// <summary>
        /// 引数なしで表示し、閉じるまで待って結果を受け取る
        /// (例: 入力フィールドを動的生成し、入力値だけを受け取る UI)。
        /// 内部で Unit を詰めるだけで、挙動は上と同一。呼び側は結果型だけを知ればよい。
        /// </summary>
        UniTask<UiResult<TResult>> ShowForResultAsync<TResult>(string key, CancellationToken ct = default);

        /// <summary>
        /// 結果が要らない表示。入場演出の完了 (Shown) で await が返る。
        /// 閉じる処理 (退場演出〜解放) は基盤が裏で面倒を見る。
        /// </summary>
        UniTask ShowAsync<TArgs>(string key, TArgs args, CancellationToken ct = default);

        /// <summary>
        /// 表示引数もない表示 (文脈フリー層)。UiPage / UiEntryPoint (非ジェネリック) 向け。
        /// 内部で Unit を詰めるだけで、挙動は ShowAsync&lt;TArgs&gt; と同一。
        /// </summary>
        UniTask ShowAsync(string key, CancellationToken ct = default);

        /// <summary>
        /// 開いた本人以外が閉じるための経路 (画面遷移で全部畳む等)。
        /// 対象の ShowAsync はキャンセル (HasValue=false) で返る。
        /// 退場演出と解放の完了まで待つ。
        /// </summary>
        UniTask HideAsync(string key, CancellationToken ct = default);

        bool IsShown(string key);
    }
}
