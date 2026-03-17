using System.ComponentModel.DataAnnotations;
using ReplicaSync.Core.Enums;

namespace ReplicaSync.Core.Models;

/// <summary>
/// Represents a sync definition that configures how data is synchronized between Pleasanter instances.
/// </summary>
public class SyncDefinition
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the sync definition identifier (e.g., "master-employee").</summary>
    [Required]
    [MaxLength(100)]
    public string SyncId { get; set; } = string.Empty;

    /// <summary>Gets or sets a description of this sync definition.</summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the sync topology type.</summary>
    public TopologyType Topology { get; set; } = TopologyType.HubSpoke;

    /// <summary>Gets or sets the conflict resolution strategy.</summary>
    public ConflictResolutionStrategy ConflictResolution { get; set; } = ConflictResolutionStrategy.SourceWins;

    /// <summary>Gets or sets the change detection method.</summary>
    public ChangeDetectionMethod ChangeDetectionMethod { get; set; } = ChangeDetectionMethod.Polling;

    /// <summary>Gets or sets the polling interval in seconds.</summary>
    [Range(1, 3600)]
    public int PollingIntervalSeconds { get; set; } = 5;

    /// <summary>Gets or sets the sync user ID used in Pleasanter.</summary>
    public int SyncUserId { get; set; } = 1;

    /// <summary>Gets or sets the sync user name.</summary>
    [MaxLength(100)]
    public string SyncUserName { get; set; } = "SyncService";

    /// <summary>Gets or sets the source instance ID (FK).</summary>
    public int SourceInstanceId { get; set; }

    /// <summary>Gets or sets the source instance.</summary>
    public SyncInstance? SourceInstance { get; set; }

    /// <summary>Gets or sets the source site ID in Pleasanter.</summary>
    public long SourceSiteId { get; set; }

    /// <summary>Gets or sets whether this sync definition is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets the sync key columns (comma-separated, e.g., "ClassA").</summary>
    [Required]
    [MaxLength(500)]
    public string SyncKeyColumns { get; set; } = "ClassA";

    /// <summary>Gets or sets the default included columns (comma-separated).</summary>
    [MaxLength(2000)]
    public string IncludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the default excluded columns (comma-separated).</summary>
    [MaxLength(2000)]
    public string ExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the record filter include conditions as JSON (e.g., {"ClassB": "approved"}).</summary>
    [MaxLength(4000)]
    public string? RecordFilterInclude { get; set; }

    /// <summary>Gets or sets the record filter exclude conditions as JSON (e.g., {"ClassC": "confidential"}).</summary>
    [MaxLength(4000)]
    public string? RecordFilterExclude { get; set; }

    /// <summary>Gets or sets whether attachment (Binaries) sync is enabled.</summary>
    public bool AttachmentsEnabled { get; set; }

    /// <summary>Gets or sets the attachment storage type (only "Rds" is supported).</summary>
    [MaxLength(50)]
    public string AttachmentsStorageType { get; set; } = "Rds";

    /// <summary>Gets or sets the target mappings.</summary>
    public ICollection<SyncTargetMapping> TargetMappings { get; set; } = new List<SyncTargetMapping>();

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets whether version history is enabled for this sync definition.</summary>
    public bool VersionHistoryEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of versions to retain per record.
    /// When null, no version count limit is applied. Default is 20.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? VersionHistoryMaxVersions { get; set; } = 20;

    /// <summary>
    /// Gets or sets the maximum number of days to retain version history.
    /// When null, no age limit is applied. Default is 180 days.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? VersionHistoryMaxDays { get; set; } = 180;

    /// <summary>
    /// Gets the sync key column names as a list.
    /// </summary>
    public IReadOnlyList<string> GetSyncKeyColumnList()
    {
        if (string.IsNullOrWhiteSpace(SyncKeyColumns))
        {
            return [];
        }

        return SyncKeyColumns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Gets the include column names as a list.
    /// </summary>
    public IReadOnlyList<string> GetIncludeColumnList()
    {
        if (string.IsNullOrWhiteSpace(IncludeColumns))
        {
            return [];
        }

        return IncludeColumns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Gets the exclude column names as a list.
    /// </summary>
    public IReadOnlyList<string> GetExcludeColumnList()
    {
        if (string.IsNullOrWhiteSpace(ExcludeColumns))
        {
            return [];
        }

        return ExcludeColumns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
