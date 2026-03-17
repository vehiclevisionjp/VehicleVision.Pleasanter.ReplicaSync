using System.ComponentModel.DataAnnotations;

namespace ReplicaSync.Core.Models;

/// <summary>
/// Maps a sync definition to a target instance with direction and column overrides.
/// </summary>
public class SyncTargetMapping
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the parent sync definition ID.</summary>
    public int SyncDefinitionId { get; set; }

    /// <summary>Gets or sets the parent sync definition.</summary>
    public SyncDefinition? SyncDefinition { get; set; }

    /// <summary>Gets or sets the target instance ID (FK).</summary>
    public int TargetInstanceId { get; set; }

    /// <summary>Gets or sets the target instance.</summary>
    public SyncInstance? TargetInstance { get; set; }

    /// <summary>Gets or sets the target site ID in Pleasanter.</summary>
    public long TargetSiteId { get; set; }

    /// <summary>Gets or sets whether source-to-target sync is enabled.</summary>
    public bool SourceToTargetEnabled { get; set; } = true;

    /// <summary>Gets or sets whether target-to-source sync is enabled.</summary>
    public bool TargetToSourceEnabled { get; set; }

    /// <summary>Gets or sets columns excluded from target-to-source sync (comma-separated).</summary>
    [MaxLength(2000)]
    public string TargetToSourceExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets columns excluded for this specific target (comma-separated).</summary>
    [MaxLength(2000)]
    public string TargetExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the record filter include override for this target as JSON.</summary>
    [MaxLength(4000)]
    public string? RecordFilterIncludeOverride { get; set; }

    /// <summary>Gets or sets the record filter exclude override for this target as JSON.</summary>
    [MaxLength(4000)]
    public string? RecordFilterExcludeOverride { get; set; }
}
