using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using ReplicaSync.Core.Enums;
using ReplicaSync.Core.Interfaces;
using ReplicaSync.Core.Models;
using ReplicaSync.Infrastructure.Data;
using ReplicaSync.Infrastructure.Extensions;
using ReplicaSync.Web.Components;
using ReplicaSync.Web.Security;

// パスワードハッシュ生成 CLI: dotnet run -- --hash-password <password>
if (args.Length >= 2 && args[0] == "--hash-password")
{
    Console.WriteLine(PasswordHasher.HashPassword(args[1]));
    return;
}

// NLog の早期初期化（起動エラーも NLog でキャプチャ）
var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 既定のロギングプロバイダーをクリアし、NLog に一本化
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Security settings
    builder.Services.Configure<SecuritySettings>(
        builder.Configuration.GetSection("Security"));

    // Authentication: Cookie (Web UI) + ApiKey (WebAPI)
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
            options.Cookie.SameSite = SameSiteMode.Strict;
        })
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationHandler.SchemeName, _ => { });

    builder.Services.AddAuthorization();
    builder.Services.AddCascadingAuthenticationState();

    // Add Blazor services
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Configure the sync config database
    var configDbConnection = builder.Configuration.GetConnectionString("ConfigDatabase")
        ?? "Data Source=ReplicaSync.db";
    var configDbTypeStr = builder.Configuration.GetValue<string>("ConfigDatabaseType") ?? "SqlServer";
    var configDbType = Enum.Parse<DbmsType>(configDbTypeStr, ignoreCase: true);

    builder.Services.AddReplicaSyncInfrastructure(configDbConnection, configDbType);

    var app = builder.Build();

    // Auto-create database on startup
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Seed initial admin user if no users exist
        var userRepo = scope.ServiceProvider.GetRequiredService<IAppUserRepository>();
        if (!await userRepo.AnyUsersExistAsync())
        {
            var securityConfig = builder.Configuration.GetSection("Security").Get<SecuritySettings>() ?? new SecuritySettings();
            var initialUser = new AppUser
            {
                Username = securityConfig.InitialAdminUsername,
                PasswordHash = PasswordHasher.HashPassword(securityConfig.InitialAdminPassword),
                MustChangePassword = true,
            };
            await userRepo.CreateAsync(initialUser);
            logger.Info("初期管理者ユーザー '{Username}' を作成しました。初回ログイン時にパスワード変更が必要です。", initialUser.Username);
        }
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
    }

    // IP address restriction (must be early in the pipeline)
    app.UseMiddleware<IpWhitelistMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    // Redirect users who must change their password
    app.UseMiddleware<MustChangePasswordMiddleware>();

    app.UseAntiforgery();
    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    LogManager.Shutdown();
}
