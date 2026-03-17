using ReplicaSync.Core.Enums;
using ReplicaSync.Core.Models;
using ReplicaSync.Core.Services;

namespace ReplicaSync.Core.Tests.Services;

public class SyncRuleEngineTests
{
    private const int ExpectedTotalDataColumnCount = 106; // 2 (Title, Body) + 26 * 4 (Class, Num, Date, Description)

    private static SyncDefinition CreateDefinition(
        string includeColumns = "",
        string excludeColumns = "") =>
        new()
        {
            SyncId = "test-sync",
            IncludeColumns = includeColumns,
            ExcludeColumns = excludeColumns,
        };

    private static SyncTargetMapping CreateTargetMapping(string targetExcludeColumns = "") =>
        new()
        {
            TargetSiteId = 100,
            TargetExcludeColumns = targetExcludeColumns,
        };

    [Fact]
    public void GetAllPleasanterDataColumnsShouldReturnCorrectCount()
    {
        // Arrange & Act
        var columns = SyncRuleEngine.GetAllPleasanterDataColumns();

        // Assert
        Assert.Equal(ExpectedTotalDataColumnCount, columns.Count);
    }

    [Fact]
    public void GetAllPleasanterDataColumnsShouldStartWithTitleAndBody()
    {
        // Arrange & Act
        var columns = SyncRuleEngine.GetAllPleasanterDataColumns();

        // Assert
        Assert.Equal("Title", columns[0]);
        Assert.Equal("Body", columns[1]);
    }

    [Fact]
    public void GetEffectiveColumnsShouldReturnAllColumnsWhenIncludeListIsEmpty()
    {
        // Arrange
        var definition = CreateDefinition();
        var targetMapping = CreateTargetMapping();

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping);

        // Assert
        Assert.Equal(ExpectedTotalDataColumnCount, result.Count);
    }

    [Fact]
    public void GetEffectiveColumnsShouldReturnOnlyIncludedColumnsWhenSpecified()
    {
        // Arrange
        var definition = CreateDefinition(includeColumns: "Title,ClassA,NumB");
        var targetMapping = CreateTargetMapping();

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("Title", result);
        Assert.Contains("ClassA", result);
        Assert.Contains("NumB", result);
    }

    [Fact]
    public void GetEffectiveColumnsShouldApplyDefinitionLevelExcludes()
    {
        // Arrange
        var definition = CreateDefinition(excludeColumns: "Body,ClassZ");
        var targetMapping = CreateTargetMapping();

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping);

        // Assert
        var expectedCount = ExpectedTotalDataColumnCount - 2;
        Assert.Equal(expectedCount, result.Count);
        Assert.DoesNotContain("Body", result);
        Assert.DoesNotContain("ClassZ", result);
    }

    [Fact]
    public void GetEffectiveColumnsShouldApplyTargetLevelExcludes()
    {
        // Arrange
        var definition = CreateDefinition();
        var targetMapping = CreateTargetMapping(targetExcludeColumns: "NumA,DateB");

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping);

        // Assert
        var expectedCount = ExpectedTotalDataColumnCount - 2;
        Assert.Equal(expectedCount, result.Count);
        Assert.DoesNotContain("NumA", result);
        Assert.DoesNotContain("DateB", result);
    }

    [Fact]
    public void GetEffectiveColumnsShouldApplyBothDefinitionAndTargetExcludes()
    {
        // Arrange
        var definition = CreateDefinition(excludeColumns: "Body");
        var targetMapping = CreateTargetMapping(targetExcludeColumns: "ClassA");

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping);

        // Assert
        var expectedCount = ExpectedTotalDataColumnCount - 2;
        Assert.Equal(expectedCount, result.Count);
        Assert.DoesNotContain("Body", result);
        Assert.DoesNotContain("ClassA", result);
    }

    [Fact]
    public void GetEffectiveColumnsShouldThrowWhenDefinitionIsNull()
    {
        // Arrange
        var targetMapping = CreateTargetMapping();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SyncRuleEngine.GetEffectiveColumns(null!, targetMapping));
    }

    [Fact]
    public void GetEffectiveColumnsShouldThrowWhenTargetMappingIsNull()
    {
        // Arrange
        var definition = CreateDefinition();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SyncRuleEngine.GetEffectiveColumns(definition, null!));
    }

    [Fact]
    public void FilterRecordColumnsShouldKeepOnlyEffectiveColumns()
    {
        // Arrange
        var sourceRecord = new PleasanterRecord
        {
            RecordId = 1,
            SiteId = 100,
            Title = "Test Title",
            Body = "Test Body",
            ColumnValues = new Dictionary<string, object?>
            {
                ["ClassA"] = "Value A",
                ["ClassB"] = "Value B",
                ["NumA"] = 42,
            },
        };
        var effectiveColumns = new List<string> { "Title", "ClassA" }.AsReadOnly();

        // Act
        var result = SyncRuleEngine.FilterRecordColumns(sourceRecord, effectiveColumns);

        // Assert
        Assert.Single(result.ColumnValues);
        Assert.Equal("Value A", result.ColumnValues["ClassA"]);
        Assert.False(result.ColumnValues.ContainsKey("ClassB"));
        Assert.False(result.ColumnValues.ContainsKey("NumA"));
    }

    [Fact]
    public void FilterRecordColumnsShouldPreserveCoreFields()
    {
        // Arrange
        var createdTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var updatedTime = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var sourceRecord = new PleasanterRecord
        {
            RecordId = 42,
            SiteId = 100,
            Ver = 3,
            Creator = 1,
            Updator = 2,
            CreatedTime = createdTime,
            UpdatedTime = updatedTime,
            IsDeleted = false,
        };
        var effectiveColumns = new List<string> { "Title" }.AsReadOnly();

        // Act
        var result = SyncRuleEngine.FilterRecordColumns(sourceRecord, effectiveColumns);

        // Assert
        Assert.Equal(42L, result.RecordId);
        Assert.Equal(100L, result.SiteId);
        Assert.Equal(3, result.Ver);
        Assert.Equal(1, result.Creator);
        Assert.Equal(2, result.Updator);
        Assert.Equal(createdTime, result.CreatedTime);
        Assert.Equal(updatedTime, result.UpdatedTime);
        Assert.False(result.IsDeleted);
    }

    [Fact]
    public void FilterRecordColumnsShouldClearTitleWhenNotInEffectiveColumns()
    {
        // Arrange
        var sourceRecord = new PleasanterRecord
        {
            Title = "Important Title",
            Body = "Some Body",
        };
        var effectiveColumns = new List<string> { "ClassA" }.AsReadOnly();

        // Act
        var result = SyncRuleEngine.FilterRecordColumns(sourceRecord, effectiveColumns);

        // Assert
        Assert.Equal(string.Empty, result.Title);
        Assert.Equal(string.Empty, result.Body);
    }

    [Fact]
    public void FilterRecordColumnsShouldPreserveTitleWhenInEffectiveColumns()
    {
        // Arrange
        var sourceRecord = new PleasanterRecord
        {
            Title = "Important Title",
            Body = "Some Body",
        };
        var effectiveColumns = new List<string> { "Title", "Body" }.AsReadOnly();

        // Act
        var result = SyncRuleEngine.FilterRecordColumns(sourceRecord, effectiveColumns);

        // Assert
        Assert.Equal("Important Title", result.Title);
        Assert.Equal("Some Body", result.Body);
    }

    [Fact]
    public void FilterRecordColumnsShouldThrowWhenSourceRecordIsNull()
    {
        // Arrange
        var effectiveColumns = new List<string> { "Title" }.AsReadOnly();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SyncRuleEngine.FilterRecordColumns(null!, effectiveColumns));
    }

    [Fact]
    public void FilterRecordColumnsShouldThrowWhenEffectiveColumnsIsNull()
    {
        // Arrange
        var sourceRecord = new PleasanterRecord();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SyncRuleEngine.FilterRecordColumns(sourceRecord, null!));
    }

    [Fact]
    public void GetWikiDataColumnsShouldReturnOnlyTitleAndBody()
    {
        // Arrange & Act
        var columns = SyncRuleEngine.GetWikiDataColumns();

        // Assert
        Assert.Equal(2, columns.Count);
        Assert.Equal("Title", columns[0]);
        Assert.Equal("Body", columns[1]);
    }

    [Fact]
    public void GetEffectiveColumnsShouldReturnWikiColumnsForWikisReferenceType()
    {
        // Arrange
        var definition = CreateDefinition();
        var targetMapping = CreateTargetMapping();

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping, ReferenceType.Wikis);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("Title", result);
        Assert.Contains("Body", result);
    }

    [Fact]
    public void GetEffectiveColumnsShouldApplyExcludesToWikiColumns()
    {
        // Arrange
        var definition = CreateDefinition(excludeColumns: "Body");
        var targetMapping = CreateTargetMapping();

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping, ReferenceType.Wikis);

        // Assert
        Assert.Single(result);
        Assert.Contains("Title", result);
    }

    [Fact]
    public void FilterRecordColumnsShouldPreserveLockedField()
    {
        // Arrange
        var sourceRecord = new PleasanterRecord
        {
            Title = "Wiki Title",
            Body = "Wiki Body",
            Locked = true,
        };
        var effectiveColumns = new List<string> { "Title", "Body" }.AsReadOnly();

        // Act
        var result = SyncRuleEngine.FilterRecordColumns(sourceRecord, effectiveColumns);

        // Assert
        Assert.True(result.Locked);
    }

    [Fact]
    public void GetEffectiveColumnsShouldReturnAllColumnsForIssuesReferenceType()
    {
        // Arrange
        var definition = CreateDefinition();
        var targetMapping = CreateTargetMapping();

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping, ReferenceType.Issues);

        // Assert
        Assert.Equal(ExpectedTotalDataColumnCount, result.Count);
    }

    [Fact]
    public void GetEffectiveColumnsShouldDefaultToResultsReferenceType()
    {
        // Arrange
        var definition = CreateDefinition();
        var targetMapping = CreateTargetMapping();

        // Act — call without referenceType (should default to Results)
        var withDefault = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping);
        var withExplicit = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping, ReferenceType.Results);

        // Assert
        Assert.Equal(withExplicit.Count, withDefault.Count);
    }

    [Fact]
    public void GetEffectiveColumnsShouldIgnoreNonWikiColumnsInIncludeForWikis()
    {
        // Arrange — include columns that don't exist in Wikis (ClassA, NumB)
        var definition = CreateDefinition(includeColumns: "Title,ClassA,NumB");
        var targetMapping = CreateTargetMapping();

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping, ReferenceType.Wikis);

        // Assert — IncludeColumns overrides the full column set, so ClassA/NumB pass through
        Assert.Equal(3, result.Count);
        Assert.Contains("Title", result);
        Assert.Contains("ClassA", result);
        Assert.Contains("NumB", result);
    }

    [Fact]
    public void GetEffectiveColumnsShouldApplyTargetExcludesToWikiColumns()
    {
        // Arrange
        var definition = CreateDefinition();
        var targetMapping = CreateTargetMapping(targetExcludeColumns: "Title");

        // Act
        var result = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping, ReferenceType.Wikis);

        // Assert
        Assert.Single(result);
        Assert.Contains("Body", result);
    }

    [Fact]
    public void FilterRecordColumnsShouldPreserveIsDeletedTrue()
    {
        // Arrange
        var sourceRecord = new PleasanterRecord
        {
            RecordId = 1,
            IsDeleted = true,
        };
        var effectiveColumns = new List<string> { "Title" }.AsReadOnly();

        // Act
        var result = SyncRuleEngine.FilterRecordColumns(sourceRecord, effectiveColumns);

        // Assert
        Assert.True(result.IsDeleted);
    }

    [Fact]
    public void FilterRecordColumnsShouldPreserveLockedFalse()
    {
        // Arrange
        var sourceRecord = new PleasanterRecord
        {
            Title = "Test",
            Locked = false,
        };
        var effectiveColumns = new List<string> { "Title" }.AsReadOnly();

        // Act
        var result = SyncRuleEngine.FilterRecordColumns(sourceRecord, effectiveColumns);

        // Assert
        Assert.False(result.Locked);
    }

    [Fact]
    public void GetAllPleasanterDataColumnsShouldContainExpectedColumns()
    {
        // Arrange & Act
        var columns = SyncRuleEngine.GetAllPleasanterDataColumns();

        // Assert
        Assert.Contains("ClassA", columns);
        Assert.Contains("ClassZ", columns);
        Assert.Contains("NumA", columns);
        Assert.Contains("NumZ", columns);
        Assert.Contains("DateA", columns);
        Assert.Contains("DateZ", columns);
        Assert.Contains("DescriptionA", columns);
        Assert.Contains("DescriptionZ", columns);
    }

    [Fact]
    public void GetWikiDataColumnsShouldNotContainClassNumDateDescription()
    {
        // Arrange & Act
        var columns = SyncRuleEngine.GetWikiDataColumns();

        // Assert
        Assert.DoesNotContain("ClassA", columns);
        Assert.DoesNotContain("NumA", columns);
        Assert.DoesNotContain("DateA", columns);
        Assert.DoesNotContain("DescriptionA", columns);
    }
}
