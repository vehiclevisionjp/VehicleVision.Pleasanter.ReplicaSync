using ReplicaSync.Core.Models;

namespace ReplicaSync.Core.Tests.Models;

public class PleasanterRecordTests
{
    [Fact]
    public void GetColumnValueShouldReturnNullForMissingColumn()
    {
        // Arrange
        var record = new PleasanterRecord();

        // Act
        var result = record.GetColumnValue("ClassA");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetColumnValueShouldReturnValueForExistingColumn()
    {
        // Arrange
        var expectedValue = "TestValue";
        var record = new PleasanterRecord
        {
            ColumnValues = new Dictionary<string, object?> { ["ClassA"] = expectedValue }
        };

        // Act
        var result = record.GetColumnValue("ClassA");

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void GetColumnValueShouldReturnNullWhenValueIsExplicitlyNull()
    {
        // Arrange
        var record = new PleasanterRecord
        {
            ColumnValues = new Dictionary<string, object?> { ["ClassA"] = null }
        };

        // Act
        var result = record.GetColumnValue("ClassA");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ColumnValuesShouldStoreAndRetrieveStringValue()
    {
        // Arrange
        var record = new PleasanterRecord();
        var expectedValue = "Hello";

        // Act
        record.ColumnValues["ClassA"] = expectedValue;

        // Assert
        Assert.Equal(expectedValue, record.GetColumnValue("ClassA"));
    }

    [Fact]
    public void ColumnValuesShouldStoreAndRetrieveNumericValue()
    {
        // Arrange
        var record = new PleasanterRecord();
        var expectedValue = 42.5m;

        // Act
        record.ColumnValues["NumA"] = expectedValue;

        // Assert
        Assert.Equal(expectedValue, record.GetColumnValue("NumA"));
    }

    [Fact]
    public void ColumnValuesShouldStoreAndRetrieveDateTimeValue()
    {
        // Arrange
        var record = new PleasanterRecord();
        var expectedValue = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        record.ColumnValues["DateA"] = expectedValue;

        // Assert
        Assert.Equal(expectedValue, record.GetColumnValue("DateA"));
    }

    [Fact]
    public void DefaultValuesShouldBeSetCorrectly()
    {
        // Arrange & Act
        var record = new PleasanterRecord();

        // Assert
        Assert.Equal(0L, record.RecordId);
        Assert.Equal(0L, record.SiteId);
        Assert.Equal(0, record.Ver);
        Assert.Equal(string.Empty, record.Title);
        Assert.Equal(string.Empty, record.Body);
        Assert.Equal(0, record.Creator);
        Assert.Equal(0, record.Updator);
        Assert.False(record.Locked);
        Assert.False(record.IsDeleted);
        Assert.NotNull(record.ColumnValues);
        Assert.Empty(record.ColumnValues);
    }
}
