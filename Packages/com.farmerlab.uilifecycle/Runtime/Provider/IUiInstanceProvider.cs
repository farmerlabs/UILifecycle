using System.Threading;
using Cysharp.Threading.Tasks;

namespace UiLifecycle
{
    /// <summary>
    /// 実体の調達 / 解放の差し込み口。
    /// 基盤は Unity 標準だけで書ける実装 (SceneObject / Prefab / AdditiveScene) を持つ。
    /// 外部依存が要る調達 (Addressables 等) は基盤に入れない — 依存が乗るため
    /// (Feel を基盤から外したのと同じ判断)。プロダクト側で実装し
    /// UiHost.RegisterProvider(id, provider) + Kind=Custom で差す。
    /// </summary>
    public interface IUiInstanceProvider
    {
        /// <summary>
        /// 実体を調達する。返す実体は「非表示」であること。
        ///
        /// UiHost は 調達 → Construct(args) → EnterAsync (表示 + 入場演出) の順で進める。
        /// 調達直後に 1 フレームでも表示されていると、
        ///   ・入場演出が初期値 (alpha=0 等) を入れる前に素の状態が見える
        ///   ・Start が Construct を追い越す (「Construct は Start より前」が崩れる)
        /// の 2 つが同時に起きる。
        ///
        /// PrefabProvider は Instantiate が同期完了するため自動的に満たす。
        /// フレームを跨ぐ調達 (Additive Scene / Addressables) は明示的に落とすこと。
        /// </summary>
        UniTask<IUiEntryPoint> AcquireAsync(CancellationToken ct);

        /// <summary>
        /// 実体を解放する。Transient のときだけ、退場演出の完了後に呼ばれる。
        /// entryPoint は Acquire が返したもの — 解放に要る情報は Root から復元できるため、
        /// Provider 自身が調達済みの実体を覚えておく必要はない。
        /// </summary>
        UniTask ReleaseAsync(IUiEntryPoint entryPoint, CancellationToken ct);
    }
}
