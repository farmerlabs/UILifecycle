using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace UiLifecycle.Tests
{
    /// <summary>
    /// PlayMode テスト。この基盤の核心主張を検証する:
    ///  1. 表示ごとの引数注入で 3 ポリシーの挙動が揃う
    ///  2. Construct は Start より前
    ///  3. 結果は ShowForResultAsync の戻り値 (配線ゼロ)
    ///  4. 再入は無視、ただし await は必ず返る
    ///  5. Hide はキャンセル扱い (調達がフレームを跨いでいる最中でも同じ)
    ///  6. 寿命と供給手段の病的な組み合わせは実行時に弾かれる
    /// </summary>
    public class UiHostTests
    {
        private GameObject _template;

        [SetUp]
        public void SetUp()
        {
            // 非アクティブなテンプレート = Prefab 相当 (Instantiate 元)
            _template = new GameObject("Template");
            _template.SetActive(false);
            _template.AddComponent<TestPage>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var page in Object.FindObjectsOfType<TestPage>(true))
                Object.DestroyImmediate(page.gameObject);
            if (_template != null) Object.DestroyImmediate(_template);
        }

        private UiHost CreateHost(LifetimePolicy policy, string key = "Page")
        {
            var registry = UiRegistryAsset.CreateForTest(
                UiRegistryEntry.CreateForTest(key, policy, ProviderKind.Prefab, _template));
            return new UiHost(registry);
        }

        private static TestPage FindActivePage()
        {
            foreach (var page in Object.FindObjectsOfType<TestPage>())
                if (page.gameObject.activeInHierarchy) return page;
            return null;
        }

        // ─────────────────────────────────────────

        [UnityTest]
        public IEnumerator Transient_結果を返して閉じると_値が届き実体が破棄される() => UniTask.ToCoroutine(async () =>
        {
            var host = CreateHost(LifetimePolicy.Transient);

            var showTask = host.ShowForResultAsync<TestArgs, string>("Page", new TestArgs(7));
            var page = FindActivePage();
            Assert.That(page, Is.Not.Null, "Show 直後に実体が表示されている");
            Assert.That(page.LastArgs?.Id, Is.EqualTo(7), "表示引数が OnShow に届いている");
            Assert.That(host.IsShown("Page"), Is.True);

            page.DoClose("done");
            var result = await showTask;

            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.EqualTo("done"), "結果が ShowForResultAsync の戻り値で届く");

            await UniTask.Yield(); // Destroy はフレーム末
            Assert.That(page == null, Is.True, "Transient は閉じたら破棄される");
            Assert.That(host.IsShown("Page"), Is.False);
        });

        [UnityTest]
        public IEnumerator Cached_再表示で同一実体が再利用され_OnShowが毎回呼ばれる() => UniTask.ToCoroutine(async () =>
        {
            var host = CreateHost(LifetimePolicy.Cached);

            var t1 = host.ShowForResultAsync<TestArgs, string>("Page", new TestArgs(1));
            var first = FindActivePage();
            first.DoClose("a");
            await t1;
            await UniTask.Yield();

            Assert.That(first == null, Is.False, "Cached は閉じても破棄されない");
            Assert.That(first.gameObject.activeSelf, Is.False, "閉じたら非表示");

            var t2 = host.ShowForResultAsync<TestArgs, string>("Page", new TestArgs(2));
            var second = FindActivePage();

            Assert.That(ReferenceEquals(first, second), Is.True, "同一実体の再利用");
            Assert.That(second.OnShowCount, Is.EqualTo(2), "表示ごとに OnShow が呼ばれる");
            Assert.That(second.LastArgs?.Id, Is.EqualTo(2), "再表示でも新しい引数が入る (再表示=初期状態)");

            second.DoClose("b");
            await t2;
        });

        [UnityTest]
        public IEnumerator Construct_はStartより前に呼ばれる() => UniTask.ToCoroutine(async () =>
        {
            var host = CreateHost(LifetimePolicy.Transient);

            var showTask = host.ShowForResultAsync<TestArgs, string>("Page", new TestArgs(1));
            var page = FindActivePage();

            await UniTask.Yield(); // Start が走るフレームまで進める
            await UniTask.Yield();

            var onShowIndex = page.EventLog.IndexOf("OnShow(1)");
            var startIndex = page.EventLog.IndexOf("Start");
            Assert.That(onShowIndex, Is.GreaterThanOrEqualTo(0), "OnShow が呼ばれている");
            Assert.That(startIndex, Is.GreaterThanOrEqualTo(0), "Start が呼ばれている");
            Assert.That(onShowIndex, Is.LessThan(startIndex), "OnShow (Construct) は Start より前");

            page.DoCancel();
            await showTask;
        });

        [UnityTest]
        public IEnumerator 再入は無視され_awaitは即キャンセルで返る() => UniTask.ToCoroutine(async () =>
        {
            var host = CreateHost(LifetimePolicy.Transient);

            var t1 = host.ShowForResultAsync<TestArgs, string>("Page", new TestArgs(1));
            var t2 = host.ShowForResultAsync<TestArgs, string>("Page", new TestArgs(2));

            var r2 = await t2; // 待たされず返ること
            Assert.That(r2.HasValue, Is.False, "再入はキャンセル扱い");

            var page = FindActivePage();
            Assert.That(page.OnShowCount, Is.EqualTo(1), "2 回目の Show は実体に届いていない");
            Assert.That(page.LastArgs?.Id, Is.EqualTo(1));

            page.DoClose("a");
            var r1 = await t1;
            Assert.That(r1.Value, Is.EqualTo("a"), "先行の表示は影響を受けない");
        });

        [UnityTest]
        public IEnumerator Hide_で開いた本人のawaitがキャンセルで返る() => UniTask.ToCoroutine(async () =>
        {
            var host = CreateHost(LifetimePolicy.Transient);

            var showTask = host.ShowForResultAsync<TestArgs, string>("Page", new TestArgs(1));
            var page = FindActivePage();

            await host.HideAsync("Page"); // 退場〜解放の完了まで待つ

            var result = await showTask;
            Assert.That(result.HasValue, Is.False, "Hide はキャンセル扱い");

            await UniTask.Yield();
            Assert.That(page == null, Is.True, "Transient は Hide でも破棄される");
        });

        [UnityTest]
        public IEnumerator 結果不要のShow_はShownで返り_閉じは裏で処理される() => UniTask.ToCoroutine(async () =>
        {
            var host = CreateHost(LifetimePolicy.Transient);

            await host.ShowAsync("Page", new TestArgs(5)); // Shown で返る
            var page = FindActivePage();
            Assert.That(host.IsShown("Page"), Is.True);
            Assert.That(page.LastArgs?.Id, Is.EqualTo(5));

            page.DoCancel();
            await UniTask.Yield();

            Assert.That(host.IsShown("Page"), Is.False, "閉じ処理は基盤が裏で完了させる");
            await UniTask.Yield();
            Assert.That(page == null, Is.True);
        });

        [UnityTest]
        public IEnumerator Persistent_シーン登録実体が再利用され_破棄されない() => UniTask.ToCoroutine(async () =>
        {
            // UiSceneAnchor 相当の自己登録 (シーン配置 UI)
            var registry = UiRegistryAsset.CreateForTest(
                UiRegistryEntry.CreateForTest("Home", LifetimePolicy.Persistent, ProviderKind.SceneObject));
            var host = new UiHost(registry);

            var sceneGo = new GameObject("Home");
            var scenePage = sceneGo.AddComponent<TestPage>();
            registry.Store.Set("Home", scenePage);

            var t1 = host.ShowForResultAsync<TestArgs, string>("Home", new TestArgs(1));
            Assert.That(scenePage.LastArgs?.Id, Is.EqualTo(1), "シーン配置でも同じ形で引数が入る");
            scenePage.DoClose("x");
            await t1;
            await UniTask.Yield();

            Assert.That(scenePage == null, Is.False, "Persistent は破棄されない");

            var t2 = host.ShowForResultAsync<TestArgs, string>("Home", new TestArgs(2));
            Assert.That(scenePage.OnShowCount, Is.EqualTo(2), "動的 UI と同じ挙動 (表示ごと注入)");
            scenePage.DoCancel();
            await t2;
        });

        [UnityTest]
        public IEnumerator 文脈フリー層_UiPageは引数なしShowで開きHideで閉じる() => UniTask.ToCoroutine(async () =>
        {
            var template = new GameObject("PlainTemplate");
            template.SetActive(false);
            template.AddComponent<UiPage>();
            var registry = UiRegistryAsset.CreateForTest(
                UiRegistryEntry.CreateForTest("Plain", LifetimePolicy.Transient, ProviderKind.Prefab, template));
            var host = new UiHost(registry);
            try
            {
                await host.ShowAsync("Plain"); // args 記述なし。Unit は内部で詰まる
                Assert.That(host.IsShown("Plain"), Is.True, "コード 0 行のページが同じ契約で開く");

                await host.HideAsync("Plain");
                Assert.That(host.IsShown("Plain"), Is.False, "第三者経路 (Hide) も同一");
            }
            finally
            {
                Object.DestroyImmediate(template);
            }
        });

        [UnityTest]
        public IEnumerator 調達中のHideは握り潰されず_awaitも必ず返る() => UniTask.ToCoroutine(async () =>
        {
            // 調達がフレームを跨ぐ Provider (Additive Scene / Addressables) でだけ開く窓。
            // Prefab は Instantiate が同期完了するため今まで表に出ていなかった。
            var provider = new FakeSlowProvider(_template);
            var registry = UiRegistryAsset.CreateForTest(
                UiRegistryEntry.CreateForTest("Slow", LifetimePolicy.Transient, ProviderKind.Custom, customProviderId: "Slow"));
            var host = new UiHost(registry);
            host.RegisterProvider("Slow", provider);

            var showTask = host.ShowForResultAsync<TestArgs, string>("Slow", new TestArgs(1));
            await UniTask.Yield();

            Assert.That(provider.IsAcquiring, Is.True, "まだ調達中 (EntryPoint が存在しない窓)");
            Assert.That(FindActivePage(), Is.Null);

            var hideTask = host.HideAsync("Slow");   // ← EntryPoint がまだ無い状態で Hide
            provider.CompleteAcquire();

            var result = await showTask;
            Assert.That(result.HasValue, Is.False, "調達中の Hide が握り潰されずキャンセルになる");

            // 修正前はここが永遠に返らなかった (誰も閉じないため Completion が立たない)
            var finished = await UniTask.WhenAny(hideTask, UniTask.Delay(1000, DelayType.Realtime));
            Assert.That(finished, Is.EqualTo(0), "HideAsync の await が返る");

            await UniTask.Yield();
            Assert.That(provider.ReleaseCount, Is.EqualTo(1), "Transient なので解放まで到達する");
            Assert.That(host.IsShown("Slow"), Is.False);
        });

        [UnityTest]
        public IEnumerator Persistent_は調達するProviderと組み合わせられない() => UniTask.ToCoroutine(async () =>
        {
            // Persistent = 「シーンに配置済み = 基盤は調達しない」なので SceneObject 専用。
            // 塞がないと store に載らないまま毎回調達され、実体が増え続ける。
            var registry = UiRegistryAsset.CreateForTest(
                UiRegistryEntry.CreateForTest("Leak", LifetimePolicy.Persistent, ProviderKind.Custom, customProviderId: "Slow"));
            var host = new UiHost(registry);

            try
            {
                await host.ShowAsync("Leak");
                Assert.Fail("例外が飛ぶはず");
            }
            catch (InvalidOperationException e)
            {
                Assert.That(e.Message, Does.Contain("Cached / Transient"), "Prefab と同じ規則・同じ体裁で誘導する");
            }
        });

        [UnityTest]
        public IEnumerator 型不一致は明確な例外になる() => UniTask.ToCoroutine(async () =>
        {
            var host = CreateHost(LifetimePolicy.Transient);

            try
            {
                await host.ShowForResultAsync<int, int>("Page", 1);
                Assert.Fail("例外が飛ぶはず");
            }
            catch (InvalidOperationException e)
            {
                Assert.That(e.Message, Does.Contain("Page"), "key 入りのメッセージで原因が追える");
            }
        });
    }
}
