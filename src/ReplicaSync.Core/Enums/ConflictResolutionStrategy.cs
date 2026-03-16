namespace ReplicaSync.Core.Enums;

/// <summary>Strategy for resolving sync conflicts</summary>
public enum ConflictResolutionStrategy
{
    /// <summary>Source (parent) data always wins</summary>
    SourceWins,

    /// <summary>Most recent update wins (by UpdatedTime)</summary>
    LastWriteWins,

    /// <summary>Log conflict for manual resolution</summary>
    ManualResolution
}
