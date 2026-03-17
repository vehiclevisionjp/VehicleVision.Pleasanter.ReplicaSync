using Microsoft.Playwright;

namespace VehicleVision.Pleasanter.ReplicaSync.Web.E2E;

/// <summary>
/// テスト間でブラウザインスタンスを共有するためのフィクスチャ。
/// </summary>
public class BrowserFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }
}

[CollectionDefinition("Screenshots")]
#pragma warning disable CA1711 // xUnit の CollectionDefinition に必要な命名
public class ScreenshotCollection : ICollectionFixture<BrowserFixture>;
#pragma warning restore CA1711

/// <summary>
/// Playwright を使用して Web UI の各画面のスクリーンショットを自動取得する。
/// 取得した画像は docs/wiki/images/manual/ に保存される。
/// </summary>
/// <remarks>
/// 前提条件:
/// - Web アプリケーションが http://localhost:5069 で起動していること
/// - 初期管理者ユーザー（administrator / vehiclevision）でログインできること
/// - Playwright ブラウザがインストール済みであること（pwsh bin/Debug/net10.0/playwright.ps1 install）
/// </remarks>
[Collection("Screenshots")]
public class ScreenshotTests : IAsyncLifetime
{
    private const string BaseUrl = "http://localhost:5069";
    private const string Username = "administrator";
    private const string InitialPassword = "vehiclevision";
    private const string NewPassword = "NewP@ssw0rd!";

    private static readonly string OutputDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "wiki", "images", "manual"));

    /// <summary>パスワード変更済みフラグ（テストインスタンス間で共有）。</summary>
    private static bool s_passwordChanged;

    private readonly BrowserFixture _fixture;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public ScreenshotTests(BrowserFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        _context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 },
            Locale = "ja-JP",
        });
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    /// <summary>ログイン画面のスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureLoginPage()
    {
        await _page.GotoAsync($"{BaseUrl}/account/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await SaveScreenshotAsync("01-login");
    }

    /// <summary>ログインしてダッシュボードのスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureDashboard()
    {
        await LoginAsync();
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorAsync();
        await SaveScreenshotAsync("02-dashboard");
    }

    /// <summary>インスタンス一覧画面のスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureInstancesIndex()
    {
        await LoginAsync();
        await _page.GotoAsync($"{BaseUrl}/instances");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorAsync();
        await SaveScreenshotAsync("03-instances-index");
    }

    /// <summary>インスタンス新規作成画面のスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureInstancesCreate()
    {
        await LoginAsync();
        await _page.GotoAsync($"{BaseUrl}/instances/create");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorAsync();
        await SaveScreenshotAsync("04-instances-create");
    }

    /// <summary>同期定義一覧画面のスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureSyncDefinitionsIndex()
    {
        await LoginAsync();
        await _page.GotoAsync($"{BaseUrl}/sync-definitions");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorAsync();
        await SaveScreenshotAsync("05-sync-definitions-index");
    }

    /// <summary>同期定義新規作成画面のスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureSyncDefinitionsCreate()
    {
        await LoginAsync();
        await _page.GotoAsync($"{BaseUrl}/sync-definitions/create");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorAsync();
        await SaveScreenshotAsync("06-sync-definitions-create");
    }

    /// <summary>同期ログ画面のスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureSyncLogsIndex()
    {
        await LoginAsync();
        await _page.GotoAsync($"{BaseUrl}/sync-logs");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorAsync();
        await SaveScreenshotAsync("07-sync-logs-index");
    }

    /// <summary>ユーザー管理画面のスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureUsersIndex()
    {
        await LoginAsync();
        await _page.GotoAsync($"{BaseUrl}/users");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorAsync();
        await SaveScreenshotAsync("08-users-index");
    }

    /// <summary>ユーザー新規作成画面のスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureUsersCreate()
    {
        await LoginAsync();
        await _page.GotoAsync($"{BaseUrl}/users/create");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorAsync();
        await SaveScreenshotAsync("09-users-create");
    }

    /// <summary>パスワード変更画面のスクリーンショットを取得する。</summary>
    [Fact]
    public async Task CaptureChangePassword()
    {
        await LoginAsync();
        await _page.GotoAsync($"{BaseUrl}/account/change-password");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await SaveScreenshotAsync("10-change-password");
    }

    /// <summary>Cookie認証でログインする。初回はパスワード変更も処理する。</summary>
    private async Task LoginAsync()
    {
        await _page.GotoAsync($"{BaseUrl}/account/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var password = s_passwordChanged ? NewPassword : InitialPassword;

        await _page.FillAsync("#username", Username);
        await _page.FillAsync("#password", password);

        // フォームPOST送信 + ナビゲーション完了を待機
        var navigationTask = _page.WaitForURLAsync(
            url => !url.Contains("/account/login"),
            new PageWaitForURLOptions { Timeout = 15000 });
        await _page.ClickAsync("button[type='submit']");
        await navigationTask;
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // MustChangePassword によるリダイレクト処理
        if (_page.Url.Contains("/account/change-password") && !s_passwordChanged)
        {
            await _page.FillAsync("#currentPassword", InitialPassword);
            await _page.FillAsync("#newPassword", NewPassword);
            await _page.FillAsync("#confirmPassword", NewPassword);

            var changeNavTask = _page.WaitForURLAsync(
                url => !url.Contains("/account/change-password"),
                new PageWaitForURLOptions { Timeout = 15000 });
            await _page.ClickAsync("button[type='submit']");
            await changeNavTask;
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            s_passwordChanged = true;
        }
    }

    /// <summary>Blazor Server の初期レンダリング完了を待機する。</summary>
    private async Task WaitForBlazorAsync()
    {
        try
        {
            await _page.WaitForSelectorAsync("text=読み込み中", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 5000,
            });
        }
        catch (TimeoutException)
        {
            // 読み込み中表示がなければ問題なし
        }
    }

    /// <summary>スクリーンショットを保存する。</summary>
    private async Task SaveScreenshotAsync(string name)
    {
        Directory.CreateDirectory(OutputDir);
        var path = Path.Combine(OutputDir, $"{name}.png");
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            FullPage = true,
        });
    }
}
