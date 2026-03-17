using System.ComponentModel.DataAnnotations;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;

namespace VehicleVision.Pleasanter.ReplicaSync.Web.Api.Models;

/// <summary>
/// Response model for a sync definition.
/// </summary>
public sealed class SyncDefinitionResponse
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the sync definition identifier.</summary>
    public string SyncId { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the sync topology type.</summary>
    public TopologyType Topology { get; set; }

    /// <summary>Gets or sets the conflict resolution strategy.</summary>
    public ConflictResolutionStrategy ConflictResolution { get; set; }

    /// <summary>Gets or sets the change detection method.</summary>
    public ChangeDetectionMethod ChangeDetectionMethod { get; set; }

    /// <summary>Gets or sets the polling interval in seconds.</summary>
    public int PollingIntervalSeconds { get; set; }

    /// <summary>Gets or sets the sync user ID.</summary>
    public int SyncUserId { get; set; }

    /// <summary>Gets or sets the sync user name.</summary>
    public string SyncUserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the source instance ID.</summary>
    public int SourceInstanceId { get; set; }

    /// <summary>Gets or sets the source site ID.</summary>
    public long SourceSiteId { get; set; }

    /// <summary>Gets or sets whether this sync definition is enabled.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Gets or sets the sync key columns.</summary>
    public string SyncKeyColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the included columns.</summary>
    public string IncludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the excluded columns.</summary>
    public string ExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the record filter include conditions.</summary>
    public string? RecordFilterInclude { get; set; }

    /// <summary>Gets or sets the record filter exclude conditions.</summary>
    public string? RecordFilterExclude { get; set; }

    /// <summary>Gets or sets whether attachment sync is enabled.</summary>
    public bool AttachmentsEnabled { get; set; }

    /// <summary>Gets or sets the attachment storage type.</summary>
    public string AttachmentsStorageType { get; set; } = string.Empty;

    /// <summary>Gets or sets whether version history is enabled.</summary>
    public bool VersionHistoryEnabled { get; set; }

    /// <summary>Gets or sets the max versions to retain.</summary>
    public int? VersionHistoryMaxVersions { get; set; }

    /// <summary>Gets or sets the max days to retain version history.</summary>
    public int? VersionHistoryMaxDays { get; set; }

    /// <summary>Gets or sets the target mappings.</summary>
    public List<SyncTargetMappingResponse> TargetMappings { get; set; } = [];

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response model for a sync target mapping.
/// </summary>
public sealed class SyncTargetMappingResponse
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the target instance ID.</summary>
    public int TargetInstanceId { get; set; }

    /// <summary>Gets or sets the target site ID.</summary>
    public long TargetSiteId { get; set; }

    /// <summary>Gets or sets whether source-to-target sync is enabled.</summary>
    public bool SourceToTargetEnabled { get; set; }

    /// <summary>Gets or sets whether target-to-source sync is enabled.</summary>
    public bool TargetToSourceEnabled { get; set; }

    /// <summary>Gets or sets the target-to-source exclude columns.</summary>
    public string TargetToSourceExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the target exclude columns.</summary>
    public string TargetExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the record filter include override.</summary>
    public string? RecordFilterIncludeOverride { get; set; }

    /// <summary>Gets or sets the record filter exclude override.</summary>
    public string? RecordFilterExcludeOverride { get; set; }
}

/// <summary>
/// Request model for creating a sync definition.
/// </summary>
public sealed class CreateSyncDefinitionRequest
{
    /// <summary>Gets or sets the sync definition identifier.</summary>
    [Required]
    [MaxLength(100)]
    public string SyncId { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
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

    /// <summary>Gets or sets the sync user ID.</summary>
    public int SyncUserId { get; set; } = 1;

    /// <summary>Gets or sets the sync user name.</summary>
    [MaxLength(100)]
    public string SyncUserName { get; set; } = "SyncService";

    /// <summary>Gets or sets the source instance ID.</summary>
    public int SourceInstanceId { get; set; }

    /// <summary>Gets or sets the source site ID.</summary>
    public long SourceSiteId { get; set; }

    /// <summary>Gets or sets whether this sync definition is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets the sync key columns.</summary>
    [Required]
    [MaxLength(500)]
    public string SyncKeyColumns { get; set; } = "ClassA";

    /// <summary>Gets or sets the included columns.</summary>
    [MaxLength(2000)]
    public string IncludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the excluded columns.</summary>
    [MaxLength(2000)]
    public string ExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the record filter include conditions.</summary>
    [MaxLength(4000)]
    public string? RecordFilterInclude { get; set; }

    /// <summary>Gets or sets the record filter exclude conditions.</summary>
    [MaxLength(4000)]
    public string? RecordFilterExclude { get; set; }

    /// <summary>Gets or sets whether attachment sync is enabled.</summary>
    public bool AttachmentsEnabled { get; set; }

    /// <summary>Gets or sets the attachment storage type.</summary>
    [MaxLength(50)]
    public string AttachmentsStorageType { get; set; } = "Rds";

    /// <summary>Gets or sets whether version history is enabled.</summary>
    public bool VersionHistoryEnabled { get; set; } = true;

    /// <summary>Gets or sets the max versions to retain.</summary>
    [Range(1, int.MaxValue)]
    public int? VersionHistoryMaxVersions { get; set; } = 20;

    /// <summary>Gets or sets the max days to retain version history.</summary>
    [Range(1, int.MaxValue)]
    public int? VersionHistoryMaxDays { get; set; } = 180;

    /// <summary>Gets or sets the target mappings.</summary>
    public List<CreateSyncTargetMappingRequest> TargetMappings { get; set; } = [];
}

/// <summary>
/// Request model for updating a sync definition.
/// </summary>
public sealed class UpdateSyncDefinitionRequest
{
    /// <summary>Gets or sets the sync definition identifier.</summary>
    [Required]
    [MaxLength(100)]
    public string SyncId { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
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

    /// <summary>Gets or sets the sync user ID.</summary>
    public int SyncUserId { get; set; } = 1;

    /// <summary>Gets or sets the sync user name.</summary>
    [MaxLength(100)]
    public string SyncUserName { get; set; } = "SyncService";

    /// <summary>Gets or sets the source instance ID.</summary>
    public int SourceInstanceId { get; set; }

    /// <summary>Gets or sets the source site ID.</summary>
    public long SourceSiteId { get; set; }

    /// <summary>Gets or sets whether this sync definition is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets the sync key columns.</summary>
    [Required]
    [MaxLength(500)]
    public string SyncKeyColumns { get; set; } = "ClassA";

    /// <summary>Gets or sets the included columns.</summary>
    [MaxLength(2000)]
    public string IncludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the excluded columns.</summary>
    [MaxLength(2000)]
    public string ExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the record filter include conditions.</summary>
    [MaxLength(4000)]
    public string? RecordFilterInclude { get; set; }

    /// <summary>Gets or sets the record filter exclude conditions.</summary>
    [MaxLength(4000)]
    public string? RecordFilterExclude { get; set; }

    /// <summary>Gets or sets whether attachment sync is enabled.</summary>
    public bool AttachmentsEnabled { get; set; }

    /// <summary>Gets or sets the attachment storage type.</summary>
    [MaxLength(50)]
    public string AttachmentsStorageType { get; set; } = "Rds";

    /// <summary>Gets or sets whether version history is enabled.</summary>
    public bool VersionHistoryEnabled { get; set; } = true;

    /// <summary>Gets or sets the max versions to retain.</summary>
    [Range(1, int.MaxValue)]
    public int? VersionHistoryMaxVersions { get; set; } = 20;

    /// <summary>Gets or sets the max days to retain version history.</summary>
    [Range(1, int.MaxValue)]
    public int? VersionHistoryMaxDays { get; set; } = 180;

    /// <summary>Gets or sets the target mappings.</summary>
    public List<CreateSyncTargetMappingRequest> TargetMappings { get; set; } = [];
}

/// <summary>
/// Request model for a sync target mapping.
/// </summary>
public sealed class CreateSyncTargetMappingRequest
{
    /// <summary>Gets or sets the target instance ID.</summary>
    public int TargetInstanceId { get; set; }

    /// <summary>Gets or sets the target site ID.</summary>
    public long TargetSiteId { get; set; }

    /// <summary>Gets or sets whether source-to-target sync is enabled.</summary>
    public bool SourceToTargetEnabled { get; set; } = true;

    /// <summary>Gets or sets whether target-to-source sync is enabled.</summary>
    public bool TargetToSourceEnabled { get; set; }

    /// <summary>Gets or sets the target-to-source exclude columns.</summary>
    [MaxLength(2000)]
    public string TargetToSourceExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the target exclude columns.</summary>
    [MaxLength(2000)]
    public string TargetExcludeColumns { get; set; } = string.Empty;

    /// <summary>Gets or sets the record filter include override.</summary>
    [MaxLength(4000)]
    public string? RecordFilterIncludeOverride { get; set; }

    /// <summary>Gets or sets the record filter exclude override.</summary>
    [MaxLength(4000)]
    public string? RecordFilterExcludeOverride { get; set; }
}

/// <summary>
/// Response model for a sync log entry.
/// </summary>
public sealed class SyncLogEntryResponse
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the sync definition ID.</summary>
    public string SyncId { get; set; } = string.Empty;

    /// <summary>Gets or sets the source instance ID.</summary>
    public string SourceInstanceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the target instance ID.</summary>
    public string TargetInstanceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the status.</summary>
    public SyncStatus Status { get; set; }

    /// <summary>Gets or sets the number of records processed.</summary>
    public int RecordsProcessed { get; set; }

    /// <summary>Gets or sets the number of records inserted.</summary>
    public int RecordsInserted { get; set; }

    /// <summary>Gets or sets the number of records updated.</summary>
    public int RecordsUpdated { get; set; }

    /// <summary>Gets or sets the number of records deleted.</summary>
    public int RecordsDeleted { get; set; }

    /// <summary>Gets or sets the number of conflicts detected.</summary>
    public int ConflictsDetected { get; set; }

    /// <summary>Gets or sets the error message.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets when the sync started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Gets or sets when the sync completed.</summary>
    public DateTime? CompletedAt { get; set; }
}
