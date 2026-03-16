using ReplicaSync.Core.Enums;
using ReplicaSync.Infrastructure.Extensions;
using ReplicaSync.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Get configuration for the config database
var configDbConnection = builder.Configuration.GetConnectionString("ConfigDatabase")
    ?? "Data Source=ReplicaSync.db";
var configDbTypeStr = builder.Configuration.GetValue<string>("ConfigDatabaseType") ?? "SqlServer";
var configDbType = Enum.Parse<DbmsType>(configDbTypeStr, ignoreCase: true);

builder.Services.AddReplicaSyncInfrastructure(configDbConnection, configDbType);
builder.Services.AddHostedService<SyncBackgroundService>();

var host = builder.Build();
host.Run();
