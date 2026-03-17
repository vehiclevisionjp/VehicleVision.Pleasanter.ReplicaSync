using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Services;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Data;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Pleasanter;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Repositories;

namespace VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ReplicaSync infrastructure services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The connection string for the configuration database.</param>
    /// <param name="configDbType">The DBMS type for the configuration database.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddReplicaSyncInfrastructure(
        this IServiceCollection services,
        string connectionString,
        DbmsType configDbType = DbmsType.SqlServer)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (configDbType)
            {
                case DbmsType.SqlServer:
                    options.UseSqlServer(connectionString);
                    break;
                case DbmsType.PostgreSql:
                    options.UseNpgsql(connectionString);
                    break;
                case DbmsType.MySql:
                    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(configDbType), configDbType, "Unsupported DBMS type.");
            }
        });

        services.AddScoped<ISyncConfigRepository, SyncConfigRepository>();
        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IVersionHistoryRepository, VersionHistoryRepository>();
        services.AddSingleton<IPleasanterDbAccess, PleasanterDbAccess>();
        services.AddScoped<VersionHistoryService>();
        services.AddScoped<ISyncEngine, SyncEngine>();

        return services;
    }
}
