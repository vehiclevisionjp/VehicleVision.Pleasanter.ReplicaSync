using Microsoft.AspNetCore.Mvc;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;
using VehicleVision.Pleasanter.ReplicaSync.Web.Api.Controllers;
using VehicleVision.Pleasanter.ReplicaSync.Web.Api.Models;

namespace VehicleVision.Pleasanter.ReplicaSync.Web.Tests.Api.Controllers;

public class SyncInstancesControllerTests
{
    private readonly StubSyncConfigRepository _repo = new();

    private SyncInstancesController CreateController() => new(_repo);

    [Fact]
    public async Task GetAllAsyncShouldReturnAllInstances()
    {
        // Arrange
        _repo.SeedInstance(new SyncInstance { Id = 1, InstanceId = "hq", DisplayName = "本社" });
        _repo.SeedInstance(new SyncInstance { Id = 2, InstanceId = "branch-a", DisplayName = "支店A" });
        var controller = CreateController();

        // Act
        var result = await controller.GetAllAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var instances = Assert.IsAssignableFrom<IEnumerable<SyncInstanceResponse>>(okResult.Value);
        Assert.Equal(2, instances.Count());
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnInstanceWhenExists()
    {
        // Arrange
        _repo.SeedInstance(new SyncInstance { Id = 1, InstanceId = "hq", DisplayName = "本社" });
        var controller = CreateController();

        // Act
        var result = await controller.GetByIdAsync(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var instance = Assert.IsType<SyncInstanceResponse>(okResult.Value);
        Assert.Equal("hq", instance.InstanceId);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.GetByIdAsync(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAsyncShouldReturnCreatedInstance()
    {
        // Arrange
        var controller = CreateController();
        var request = new CreateSyncInstanceRequest
        {
            InstanceId = "branch-b",
            DisplayName = "支店B",
            DbmsType = DbmsType.PostgreSql,
            ConnectionString = "Host=localhost;Database=test",
        };

        // Act
        var result = await controller.CreateAsync(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var instance = Assert.IsType<SyncInstanceResponse>(createdResult.Value);
        Assert.Equal("branch-b", instance.InstanceId);
        Assert.Equal(DbmsType.PostgreSql, instance.DbmsType);
    }

    [Fact]
    public async Task UpdateAsyncShouldReturnUpdatedInstance()
    {
        // Arrange
        _repo.SeedInstance(new SyncInstance
        {
            Id = 1,
            InstanceId = "hq",
            DisplayName = "本社",
            DbmsType = DbmsType.SqlServer,
            ConnectionString = "old",
        });
        var controller = CreateController();
        var request = new UpdateSyncInstanceRequest
        {
            InstanceId = "hq",
            DisplayName = "本社（更新済）",
            DbmsType = DbmsType.SqlServer,
            ConnectionString = "new",
        };

        // Act
        var result = await controller.UpdateAsync(1, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var instance = Assert.IsType<SyncInstanceResponse>(okResult.Value);
        Assert.Equal("本社（更新済）", instance.DisplayName);
    }

    [Fact]
    public async Task UpdateAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var controller = CreateController();
        var request = new UpdateSyncInstanceRequest
        {
            InstanceId = "x",
            DisplayName = "X",
            ConnectionString = "conn",
        };

        // Act
        var result = await controller.UpdateAsync(999, request, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteAsyncShouldReturnNoContentWhenExists()
    {
        // Arrange
        _repo.SeedInstance(new SyncInstance { Id = 1, InstanceId = "hq", DisplayName = "本社" });
        var controller = CreateController();

        // Act
        var result = await controller.DeleteAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.DeleteAsync(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
