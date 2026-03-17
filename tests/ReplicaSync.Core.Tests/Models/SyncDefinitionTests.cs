using ReplicaSync.Core.Enums;
using ReplicaSync.Core.Models;

namespace ReplicaSync.Core.Tests.Models;

public class SyncDefinitionTests
{
    [Fact]
    public void GetSyncKeyColumnListShouldReturnSingleItemForSingleValue()
    {
        // Arrange
        var definition = new SyncDefinition { SyncKeyColumns = "ClassA" };

        // Act
        var result = definition.GetSyncKeyColumnList();

        // Assert
        Assert.Single(result);
        Assert.Equal("ClassA", result[0]);
    }

    [Fact]
    public void GetSyncKeyColumnListShouldReturnMultipleItemsForCommaSeparatedValues()
    {
        // Arrange
        var definition = new SyncDefinition { SyncKeyColumns = "ClassA,ClassB,ClassC" };

        // Act
        var result = definition.GetSyncKeyColumnList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("ClassA", result[0]);
        Assert.Equal("ClassB", result[1]);
        Assert.Equal("ClassC", result[2]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetSyncKeyColumnListShouldReturnEmptyWhenColumnsIsEmptyOrNull(string? syncKeyColumns)
    {
        // Arrange
        var definition = new SyncDefinition { SyncKeyColumns = syncKeyColumns! };

        // Act
        var result = definition.GetSyncKeyColumnList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetSyncKeyColumnListShouldTrimWhitespace()
    {
        // Arrange
        var definition = new SyncDefinition { SyncKeyColumns = " ClassA , ClassB " };

        // Act
        var result = definition.GetSyncKeyColumnList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("ClassA", result[0]);
        Assert.Equal("ClassB", result[1]);
    }

    [Fact]
    public void GetIncludeColumnListShouldReturnCorrectList()
    {
        // Arrange
        var definition = new SyncDefinition { IncludeColumns = "Title,ClassA,NumB" };

        // Act
        var result = definition.GetIncludeColumnList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Title", result[0]);
        Assert.Equal("ClassA", result[1]);
        Assert.Equal("NumB", result[2]);
    }

    [Fact]
    public void GetIncludeColumnListShouldTrimWhitespace()
    {
        // Arrange
        var definition = new SyncDefinition { IncludeColumns = " Title , ClassA " };

        // Act
        var result = definition.GetIncludeColumnList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Title", result[0]);
        Assert.Equal("ClassA", result[1]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetIncludeColumnListShouldReturnEmptyWhenColumnsIsEmptyOrNull(string? includeColumns)
    {
        // Arrange
        var definition = new SyncDefinition { IncludeColumns = includeColumns! };

        // Act
        var result = definition.GetIncludeColumnList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetExcludeColumnListShouldReturnCorrectList()
    {
        // Arrange
        var definition = new SyncDefinition { ExcludeColumns = "Body,DescriptionZ" };

        // Act
        var result = definition.GetExcludeColumnList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Body", result[0]);
        Assert.Equal("DescriptionZ", result[1]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetExcludeColumnListShouldReturnEmptyWhenColumnsIsEmptyOrNull(string? excludeColumns)
    {
        // Arrange
        var definition = new SyncDefinition { ExcludeColumns = excludeColumns! };

        // Act
        var result = definition.GetExcludeColumnList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetExcludeColumnListShouldTrimWhitespace()
    {
        // Arrange
        var definition = new SyncDefinition { ExcludeColumns = " Body , NumA " };

        // Act
        var result = definition.GetExcludeColumnList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Body", result[0]);
        Assert.Equal("NumA", result[1]);
    }

    [Fact]
    public void DefaultValuesShouldBeSetCorrectly()
    {
        // Arrange & Act
        var definition = new SyncDefinition();

        // Assert
        Assert.Equal(string.Empty, definition.SyncId);
        Assert.Equal(string.Empty, definition.Description);
        Assert.Equal(TopologyType.HubSpoke, definition.Topology);
        Assert.Equal(ConflictResolutionStrategy.SourceWins, definition.ConflictResolution);
        Assert.Equal(ChangeDetectionMethod.Polling, definition.ChangeDetectionMethod);
        Assert.Equal(5, definition.PollingIntervalSeconds);
        Assert.Equal(1, definition.SyncUserId);
        Assert.Equal("SyncService", definition.SyncUserName);
        Assert.True(definition.IsEnabled);
        Assert.Equal("ClassA", definition.SyncKeyColumns);
        Assert.Equal(string.Empty, definition.IncludeColumns);
        Assert.Equal(string.Empty, definition.ExcludeColumns);
        Assert.Null(definition.RecordFilterInclude);
        Assert.Null(definition.RecordFilterExclude);
        Assert.False(definition.AttachmentsEnabled);
        Assert.Equal("Rds", definition.AttachmentsStorageType);
        Assert.Empty(definition.TargetMappings);
        Assert.True(definition.VersionHistoryEnabled);
        Assert.Equal(20, definition.VersionHistoryMaxVersions);
        Assert.Equal(180, definition.VersionHistoryMaxDays);
    }

    [Fact]
    public void VersionHistoryMaxVersionsShouldBeNullableForUnlimited()
    {
        // Arrange & Act
        var definition = new SyncDefinition { VersionHistoryMaxVersions = null };

        // Assert
        Assert.Null(definition.VersionHistoryMaxVersions);
    }

    [Fact]
    public void VersionHistoryMaxDaysShouldBeNullableForUnlimited()
    {
        // Arrange & Act
        var definition = new SyncDefinition { VersionHistoryMaxDays = null };

        // Assert
        Assert.Null(definition.VersionHistoryMaxDays);
    }

    [Fact]
    public void VersionHistoryCanBeDisabled()
    {
        // Arrange & Act
        var definition = new SyncDefinition { VersionHistoryEnabled = false };

        // Assert
        Assert.False(definition.VersionHistoryEnabled);
    }
}
