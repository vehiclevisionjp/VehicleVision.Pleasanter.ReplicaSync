using VehicleVision.Pleasanter.ReplicaSync.Core.Models;

namespace VehicleVision.Pleasanter.ReplicaSync.Core.Tests.Models;

public class SyncTargetMappingTests
{
    [Fact]
    public void DefaultValuesShouldBeSetCorrectly()
    {
        // Arrange & Act
        var mapping = new SyncTargetMapping();

        // Assert
        Assert.Equal(0, mapping.Id);
        Assert.Equal(0, mapping.SyncDefinitionId);
        Assert.Null(mapping.SyncDefinition);
        Assert.Equal(0, mapping.TargetInstanceId);
        Assert.Null(mapping.TargetInstance);
        Assert.Equal(0L, mapping.TargetSiteId);
        Assert.True(mapping.SourceToTargetEnabled);
        Assert.False(mapping.TargetToSourceEnabled);
        Assert.Equal(string.Empty, mapping.TargetToSourceExcludeColumns);
        Assert.Equal(string.Empty, mapping.TargetExcludeColumns);
        Assert.Null(mapping.RecordFilterIncludeOverride);
        Assert.Null(mapping.RecordFilterExcludeOverride);
    }

    [Fact]
    public void RecordFilterOverridesShouldAcceptValues()
    {
        // Arrange & Act
        var mapping = new SyncTargetMapping
        {
            RecordFilterIncludeOverride = """{"ClassB": "approved"}""",
            RecordFilterExcludeOverride = """{"ClassC": "confidential"}""",
        };

        // Assert
        Assert.Equal("""{"ClassB": "approved"}""", mapping.RecordFilterIncludeOverride);
        Assert.Equal("""{"ClassC": "confidential"}""", mapping.RecordFilterExcludeOverride);
    }
}
