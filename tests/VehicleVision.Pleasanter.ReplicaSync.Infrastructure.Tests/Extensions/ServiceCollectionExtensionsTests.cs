using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Data;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Extensions;

namespace VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Tests.Extensions;

/// <summary>
/// Tests for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    private const string DummyConnectionString = "Server=localhost;Database=test;User=test;Password=test;";

    [Theory]
    [InlineData(DbmsType.SqlServer)]
    [InlineData(DbmsType.PostgreSql)]
    [InlineData(DbmsType.MySql)]
    public void AddReplicaSyncInfrastructureShouldRegisterServicesForAllDbmsTypes(DbmsType dbmsType)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddReplicaSyncInfrastructure(DummyConnectionString, dbmsType);

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<AppDbContext>());
        Assert.NotNull(provider.GetService<ISyncConfigRepository>());
        Assert.NotNull(provider.GetService<IPleasanterDbAccess>());
        Assert.NotNull(provider.GetService<ISyncEngine>());
    }

    [Fact]
    public void AddReplicaSyncInfrastructureShouldDefaultToSqlServer()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddReplicaSyncInfrastructure(DummyConnectionString);

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<AppDbContext>());
    }

    [Theory]
    [InlineData(DbmsType.SqlServer)]
    [InlineData(DbmsType.PostgreSql)]
    [InlineData(DbmsType.MySql)]
    public void AddReplicaSyncInfrastructureShouldConfigureDbContextForDbmsType(DbmsType dbmsType)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReplicaSyncInfrastructure(DummyConnectionString, dbmsType);

        var provider = services.BuildServiceProvider();
        var context = provider.GetRequiredService<AppDbContext>();

        Assert.NotNull(context);
        Assert.NotNull(context.Database);
    }

    [Fact]
    public void AddReplicaSyncInfrastructureShouldThrowForNullServices()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() =>
            services.AddReplicaSyncInfrastructure(DummyConnectionString));
    }

    [Fact]
    public void AddReplicaSyncInfrastructureShouldThrowForNullConnectionString()
    {
        var services = new ServiceCollection();

        Assert.ThrowsAny<ArgumentException>(() =>
            services.AddReplicaSyncInfrastructure(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddReplicaSyncInfrastructureShouldThrowForEmptyOrWhitespaceConnectionString(string connectionString)
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentException>(() =>
            services.AddReplicaSyncInfrastructure(connectionString));
    }
}
