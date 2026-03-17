using Microsoft.Extensions.Logging;
using NSubstitute;
using ReplicaSync.Core.Interfaces;
using ReplicaSync.Core.Models;
using ReplicaSync.Core.Services;

namespace ReplicaSync.Core.Tests.Services;

public class VersionHistoryServiceTests
{
    private readonly IVersionHistoryRepository _repository;
    private readonly ILogger<VersionHistoryService> _logger;
    private readonly VersionHistoryService _service;

    public VersionHistoryServiceTests()
    {
        _repository = Substitute.For<IVersionHistoryRepository>();
        _logger = Substitute.For<ILogger<VersionHistoryService>>();
        _service = new VersionHistoryService(_repository, _logger);
    }

    [Fact]
    public async Task CaptureSnapshotShouldReturnNullWhenVersionHistoryDisabled()
    {
        // Arrange
        var definition = new SyncDefinition { VersionHistoryEnabled = false };
        var record = new PleasanterRecord { RecordId = 1 };

        // Act
        var result = await _service.CaptureSnapshotAsync(definition, "instance-a", 100, record);

        // Assert
        Assert.Null(result);
        await _repository.DidNotReceive().CreateAsync(Arg.Any<RecordVersionHistory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CaptureSnapshotShouldCreateEntryWithCorrectData()
    {
        // Arrange
        var definition = new SyncDefinition
        {
            SyncId = "test-sync",
            VersionHistoryEnabled = true
        };
        var updatedTime = new DateTime(2026, 3, 17, 10, 0, 0, DateTimeKind.Utc);
        var record = new PleasanterRecord
        {
            RecordId = 42,
            Title = "Test Title",
            Body = "Test Body",
            Updator = 5,
            UpdatedTime = updatedTime,
            ColumnValues = new Dictionary<string, object?> { ["ClassA"] = "value1" }
        };

        _repository.GetNextVersionNumberAsync("test-sync", "instance-a", 100, 42, Arg.Any<CancellationToken>())
            .Returns(3);
        _repository.CreateAsync(Arg.Any<RecordVersionHistory>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<RecordVersionHistory>());

        // Act
        var result = await _service.CaptureSnapshotAsync(definition, "instance-a", 100, record);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-sync", result.SyncId);
        Assert.Equal("instance-a", result.InstanceId);
        Assert.Equal(100, result.SiteId);
        Assert.Equal(42, result.RecordId);
        Assert.Equal(3, result.VersionNumber);
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("Test Body", result.Body);
        Assert.Equal(5, result.ChangedBy);
        Assert.Equal(updatedTime, result.ChangedAt);
        Assert.Contains("ClassA", result.ColumnSnapshotJson);
        Assert.False(result.IsDeleteSnapshot);
    }

    [Fact]
    public async Task CaptureSnapshotShouldCallCreateOnRepository()
    {
        // Arrange
        var definition = new SyncDefinition { SyncId = "sync-1", VersionHistoryEnabled = true };
        var record = new PleasanterRecord { RecordId = 1 };

        _repository.GetNextVersionNumberAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _repository.CreateAsync(Arg.Any<RecordVersionHistory>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<RecordVersionHistory>());

        // Act
        await _service.CaptureSnapshotAsync(definition, "inst", 10, record);

        // Assert
        await _repository.Received(1).CreateAsync(Arg.Any<RecordVersionHistory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyRetentionForRecordShouldDeleteExcessVersionsWhenMaxVersionsSet()
    {
        // Arrange
        var definition = new SyncDefinition
        {
            SyncId = "sync-1",
            VersionHistoryMaxVersions = 10
        };

        _repository.DeleteExcessVersionsAsync("sync-1", "inst-a", 100, 42, 10, Arg.Any<CancellationToken>())
            .Returns(3);

        // Act
        await _service.ApplyRetentionForRecordAsync(definition, "inst-a", 100, 42);

        // Assert
        await _repository.Received(1).DeleteExcessVersionsAsync("sync-1", "inst-a", 100, 42, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyRetentionForRecordShouldSkipWhenMaxVersionsIsNull()
    {
        // Arrange
        var definition = new SyncDefinition
        {
            SyncId = "sync-1",
            VersionHistoryMaxVersions = null
        };

        // Act
        await _service.ApplyRetentionForRecordAsync(definition, "inst-a", 100, 42);

        // Assert
        await _repository.DidNotReceive().DeleteExcessVersionsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyTimeBasedRetentionShouldDeleteOldEntriesWhenMaxDaysSet()
    {
        // Arrange
        var definition = new SyncDefinition
        {
            SyncId = "sync-1",
            VersionHistoryMaxDays = 90
        };

        _repository.DeleteOlderThanAsync("sync-1", Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(5);

        // Act
        await _service.ApplyTimeBasedRetentionAsync(definition);

        // Assert
        await _repository.Received(1).DeleteOlderThanAsync("sync-1", Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyTimeBasedRetentionShouldSkipWhenMaxDaysIsNull()
    {
        // Arrange
        var definition = new SyncDefinition
        {
            SyncId = "sync-1",
            VersionHistoryMaxDays = null
        };

        // Act
        await _service.ApplyTimeBasedRetentionAsync(definition);

        // Assert
        await _repository.DidNotReceive().DeleteOlderThanAsync(
            Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ConstructorShouldThrowWhenRepositoryIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new VersionHistoryService(null!, _logger));
    }

    [Fact]
    public void ConstructorShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new VersionHistoryService(_repository, null!));
    }

    [Fact]
    public async Task CaptureSnapshotShouldThrowWhenDefinitionIsNull()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CaptureSnapshotAsync(null!, "inst", 100, new PleasanterRecord()));
    }

    [Fact]
    public async Task CaptureSnapshotShouldThrowWhenRecordIsNull()
    {
        // Arrange
        var definition = new SyncDefinition { VersionHistoryEnabled = true };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CaptureSnapshotAsync(definition, "inst", 100, null!));
    }

    [Fact]
    public async Task CaptureDeleteSnapshotShouldCreateEntryWithIsDeleteSnapshotTrue()
    {
        // Arrange
        var definition = new SyncDefinition
        {
            SyncId = "test-sync",
            VersionHistoryEnabled = true
        };
        var record = new PleasanterRecord
        {
            RecordId = 42,
            Title = "Deleted Title",
            Body = "Deleted Body",
            Updator = 7,
            UpdatedTime = new DateTime(2026, 3, 17, 10, 0, 0, DateTimeKind.Utc),
            ColumnValues = new Dictionary<string, object?> { ["ClassA"] = "del-value" }
        };

        _repository.GetNextVersionNumberAsync("test-sync", "instance-a", 100, 42, Arg.Any<CancellationToken>())
            .Returns(5);
        _repository.CreateAsync(Arg.Any<RecordVersionHistory>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<RecordVersionHistory>());

        // Act
        var result = await _service.CaptureDeleteSnapshotAsync(definition, "instance-a", 100, record);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsDeleteSnapshot);
        Assert.Equal("test-sync", result.SyncId);
        Assert.Equal(42, result.RecordId);
        Assert.Equal(5, result.VersionNumber);
        Assert.Equal("Deleted Title", result.Title);
    }

    [Fact]
    public async Task CaptureDeleteSnapshotShouldReturnNullWhenVersionHistoryDisabled()
    {
        // Arrange
        var definition = new SyncDefinition { VersionHistoryEnabled = false };
        var record = new PleasanterRecord { RecordId = 1 };

        // Act
        var result = await _service.CaptureDeleteSnapshotAsync(definition, "instance-a", 100, record);

        // Assert
        Assert.Null(result);
        await _repository.DidNotReceive().CreateAsync(Arg.Any<RecordVersionHistory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CaptureDeleteSnapshotShouldThrowWhenDefinitionIsNull()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CaptureDeleteSnapshotAsync(null!, "inst", 100, new PleasanterRecord()));
    }

    [Fact]
    public async Task CaptureDeleteSnapshotShouldThrowWhenRecordIsNull()
    {
        // Arrange
        var definition = new SyncDefinition { VersionHistoryEnabled = true };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CaptureDeleteSnapshotAsync(definition, "inst", 100, null!));
    }
}
