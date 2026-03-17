using Microsoft.AspNetCore.Mvc;
using ReplicaSync.Core.Enums;
using ReplicaSync.Core.Interfaces;
using ReplicaSync.Core.Models;
using ReplicaSync.Web.Api.Controllers;
using ReplicaSync.Web.Api.Models;

namespace ReplicaSync.Web.Tests.Api.Controllers;

public class SyncDefinitionsControllerTests
{
    private readonly StubSyncConfigRepository _repo = new();

    private SyncDefinitionsController CreateController() => new(_repo);

    [Fact]
    public async Task GetAllAsyncShouldReturnAllDefinitions()
    {
        // Arrange
        _repo.SeedDefinition(new SyncDefinition { Id = 1, SyncId = "sync-1" });
        _repo.SeedDefinition(new SyncDefinition { Id = 2, SyncId = "sync-2" });
        var controller = CreateController();

        // Act
        var result = await controller.GetAllAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var definitions = Assert.IsAssignableFrom<IEnumerable<SyncDefinitionResponse>>(okResult.Value);
        Assert.Equal(2, definitions.Count());
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnDefinitionWhenExists()
    {
        // Arrange
        _repo.SeedDefinition(new SyncDefinition { Id = 1, SyncId = "sync-1" });
        var controller = CreateController();

        // Act
        var result = await controller.GetByIdAsync(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var definition = Assert.IsType<SyncDefinitionResponse>(okResult.Value);
        Assert.Equal("sync-1", definition.SyncId);
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
    public async Task CreateAsyncShouldReturnCreatedDefinition()
    {
        // Arrange
        var controller = CreateController();
        var request = new CreateSyncDefinitionRequest
        {
            SyncId = "new-sync",
            Description = "テスト同期定義",
            SourceInstanceId = 1,
            SourceSiteId = 100,
            TargetMappings =
            [
                new CreateSyncTargetMappingRequest
                {
                    TargetInstanceId = 2,
                    TargetSiteId = 200,
                    SourceToTargetEnabled = true,
                },
            ],
        };

        // Act
        var result = await controller.CreateAsync(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var definition = Assert.IsType<SyncDefinitionResponse>(createdResult.Value);
        Assert.Equal("new-sync", definition.SyncId);
        Assert.Single(definition.TargetMappings);
    }

    [Fact]
    public async Task UpdateAsyncShouldReturnUpdatedDefinition()
    {
        // Arrange
        _repo.SeedDefinition(new SyncDefinition { Id = 1, SyncId = "sync-1" });
        var controller = CreateController();
        var request = new UpdateSyncDefinitionRequest
        {
            SyncId = "sync-1-updated",
            Description = "更新済み",
            SourceInstanceId = 1,
            SourceSiteId = 100,
        };

        // Act
        var result = await controller.UpdateAsync(1, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var definition = Assert.IsType<SyncDefinitionResponse>(okResult.Value);
        Assert.Equal("sync-1-updated", definition.SyncId);
    }

    [Fact]
    public async Task UpdateAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var controller = CreateController();
        var request = new UpdateSyncDefinitionRequest { SyncId = "x" };

        // Act
        var result = await controller.UpdateAsync(999, request, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteAsyncShouldReturnNoContentWhenExists()
    {
        // Arrange
        _repo.SeedDefinition(new SyncDefinition { Id = 1, SyncId = "sync-1" });
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
