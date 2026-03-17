using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using Photino.NET;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Data;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Extensions;
using VehicleVision.Pleasanter.ReplicaSync.Web.Components;
using VehicleVision.Pleasanter.ReplicaSync.Web.Security;

var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    // 空きポートを自動取得して内蔵 Web サーバを起動
    var port = FindFreePort();
    var url = $"http://localhost:{port}";

    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.UseUrls(url);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // セキュリティ設定
    builder.Services.Configure<SecuritySettings>(
        builder.Configuration.GetSection("Security"));

    // Cookie 認証（デスクトップ版では API キー認証は不要）
    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole(nameof(AppRole.Administrator)));
    });
    builder.Services.AddCascadingAuthenticationState();

    // Blazor コンポーネント
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // データベース
    var configDbConnection = builder.Configuration.GetConnectionString("ConfigDatabase")
        ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ReplicaSync;Integrated Security=True";
    var configDbTypeStr = builder.Configuration.GetValue<string>("ConfigDatabaseType") ?? "SqlServer";
    var configDbType = Enum.Parse<DbmsType>(configDbTypeStr, ignoreCase: true);

    builder.Services.AddReplicaSyncInfrastructure(configDbConnection, configDbType);

    var app = builder.Build();

    // DB 自動作成・初期ユーザーシード
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var userRepo = scope.ServiceProvider.GetRequiredService<IAppUserRepository>();
        if (!await userRepo.AnyUsersExistAsync())
        {
            var securityConfig = builder.Configuration.GetSection("Security")
                .Get<SecuritySettings>() ?? new SecuritySettings();
            var initialUser = new AppUser
            {
                Username = securityConfig.InitialAdminUsername,
                PasswordHash = PasswordHasher.HashPassword(securityConfig.InitialAdminPassword),
                Role = AppRole.Administrator,
                MustChangePassword = true,
            };
            await userRepo.CreateAsync(initialUser);
            logger.Info("初期管理者ユーザー '{Username}' を作成しました。", initialUser.Username);
        }
    }

    // ミドルウェアパイプライン（デスクトップ版では IP 制限は不要）
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<MustChangePasswordMiddleware>();
    app.UseAntiforgery();
    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    // 内蔵サーバ起動
    await app.StartAsync();
    logger.Info("デスクトップアプリケーションを起動しました: {Url}", url);

    // ネイティブウィンドウを表示
    var window = new PhotinoWindow();
    window.SetTitle("ReplicaSync");
    window.SetUseOsDefaultSize(false);
    window.SetSize(1280, 900);
    window.Center();
    window.Load(url);

    window.WaitForClose();

    await app.StopAsync();
}
catch (Exception ex)
{
    logger.Error(ex, "アプリケーションが予期せず終了しました。");
    throw;
}
finally
{
    LogManager.Shutdown();
}

// OS に空きポートを問い合わせて返す
static int FindFreePort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}
