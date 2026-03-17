using NLog;
using NLog.Web;
using ReplicaSync.Core.Enums;
using ReplicaSync.Infrastructure.Data;
using ReplicaSync.Infrastructure.Extensions;
using ReplicaSync.Web.Components;
using Microsoft.EntityFrameworkCore;

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
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
    }

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
