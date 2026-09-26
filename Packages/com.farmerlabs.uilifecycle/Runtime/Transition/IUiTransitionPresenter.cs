using System.Threading;
using Cysharp.Threading.Tasks;

namespace UiLifecycle
{
    /// <summary>
    /// 演出の契約。「中身」ではなく「完了を返せること」だけを要求する。
    /// UniTask を返せれば中身は Feel でも USS でも LitMotion でも空でもよい。
    /// 基盤は Feel を知らない = Feel 未導入プロダクトでも採用できる。
    /// </summary>
    public interface IUiTransitionPresenter
    {
        UniTask PlayEnterAsync(CancellationToken ct);
        UniTask PlayExitAsync(CancellationToken ct);
    }
}
