using NLog;
using NLog.Extensions.Logging;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Extensions;
using VehicleVision.Pleasanter.ReplicaSync.Worker;

// NLog の早期初期化（起動エラーも NLog でキャプチャ）
var logger = LogManager.Setup()
    .LoadConfigurationFromFile()
    .GetCurrentClassLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // 既定のロギングプロバイダーをクリアし、NLog に一本化
    builder.Logging.ClearProviders();
    builder.Logging.AddNLog();

    // Get configuration for the config database
    var configDbConnection = builder.Configuration.GetConnectionString("ConfigDatabase")
        ?? "Data Source=ReplicaSync.db";
    var configDbTypeStr = builder.Configuration.GetValue<string>("ConfigDatabaseType") ?? "SqlServer";
    var configDbType = Enum.Parse<DbmsType>(configDbTypeStr, ignoreCase: true);

    builder.Services.AddReplicaSyncInfrastructure(configDbConnection, configDbType);
    builder.Services.AddHostedService<SyncBackgroundService>();

    var host = builder.Build();
    host.Run();
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
