using VehicleVision.Pleasanter.ReplicaSync.Core.Models;

namespace VehicleVision.Pleasanter.ReplicaSync.Core.Tests.Models;

public class RecordVersionHistoryTests
{
    [Fact]
    public void DefaultValuesShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var entry = new RecordVersionHistory();

        // Assert
        Assert.Equal(0, entry.Id);
        Assert.Equal(string.Empty, entry.SyncId);
        Assert.Equal(string.Empty, entry.InstanceId);
        Assert.Equal(0, entry.SiteId);
        Assert.Equal(0, entry.RecordId);
        Assert.Equal(0, entry.VersionNumber);
        Assert.Equal(string.Empty, entry.Title);
        Assert.Equal(string.Empty, entry.Body);
        Assert.Equal("{}", entry.ColumnSnapshotJson);
        Assert.Equal(0, entry.ChangedBy);
        Assert.False(entry.IsDeleteSnapshot);
    }

    [Fact]
    public void PropertiesShouldBeSettableAndGettable()
    {
        // Arrange
        var changedAt = new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc);
        var createdAt = new DateTime(2026, 3, 17, 12, 0, 1, DateTimeKind.Utc);

        // Act
        var entry = new RecordVersionHistory
        {
            Id = 42,
            SyncId = "master-employee",
            InstanceId = "branch-a",
            SiteId = 100,
            RecordId = 200,
            VersionNumber = 5,
            Title = "Test Title",
            Body = "Test Body",
            ColumnSnapshotJson = """{"ClassA":"value1"}""",
            ChangedBy = 10,
            ChangedAt = changedAt,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(42, entry.Id);
        Assert.Equal("master-employee", entry.SyncId);
        Assert.Equal("branch-a", entry.InstanceId);
        Assert.Equal(100, entry.SiteId);
        Assert.Equal(200, entry.RecordId);
        Assert.Equal(5, entry.VersionNumber);
        Assert.Equal("Test Title", entry.Title);
        Assert.Equal("Test Body", entry.Body);
        Assert.Equal("""{"ClassA":"value1"}""", entry.ColumnSnapshotJson);
        Assert.Equal(10, entry.ChangedBy);
        Assert.Equal(changedAt, entry.ChangedAt);
        Assert.Equal(createdAt, entry.CreatedAt);
    }

    [Fact]
    public void IsDeleteSnapshotShouldBeSettable()
    {
        // Arrange & Act
        var entry = new RecordVersionHistory { IsDeleteSnapshot = true };

        // Assert
        Assert.True(entry.IsDeleteSnapshot);
    }
}
