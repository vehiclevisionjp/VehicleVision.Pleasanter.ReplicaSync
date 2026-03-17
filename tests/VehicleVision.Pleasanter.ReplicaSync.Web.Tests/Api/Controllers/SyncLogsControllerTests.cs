using Microsoft.AspNetCore.Mvc;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;
using VehicleVision.Pleasanter.ReplicaSync.Web.Api.Controllers;
using VehicleVision.Pleasanter.ReplicaSync.Web.Api.Models;

namespace VehicleVision.Pleasanter.ReplicaSync.Web.Tests.Api.Controllers;

public class SyncLogsControllerTests
{
    private readonly StubSyncConfigRepository _repo = new();

    private SyncLogsController CreateController() => new(_repo);

    [Fact]
    public async Task GetRecentAsyncShouldReturnLogs()
    {
        // Arrange
        _repo.SeedLog(new SyncLogEntry
        {
            Id = 1,
            SyncId = "sync-1",
            Status = SyncStatus.Success,
            RecordsProcessed = 10,
            StartedAt = DateTime.UtcNow,
        });
        var controller = CreateController();

        // Act
        var result = await controller.GetRecentAsync(100, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var logs = Assert.IsAssignableFrom<IEnumerable<SyncLogEntryResponse>>(okResult.Value);
        Assert.Single(logs);
    }

    [Fact]
    public async Task GetRecentAsyncShouldReturnBadRequestWhenCountTooLarge()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.GetRecentAsync(1001, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetRecentAsyncShouldReturnBadRequestWhenCountIsZero()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.GetRecentAsync(0, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
