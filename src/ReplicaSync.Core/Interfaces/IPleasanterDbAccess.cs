using ReplicaSync.Core.Enums;
using ReplicaSync.Core.Models;

namespace ReplicaSync.Core.Interfaces;

/// <summary>
/// Interface for direct database access to Pleasanter tables.
/// </summary>
public interface IPleasanterDbAccess
{
    /// <summary>
    /// Determines the reference type (Results or Issues) for a given site.
    /// </summary>
    Task<ReferenceType> GetReferenceTypeAsync(
        string connectionString,
        DbmsType dbmsType,
        long siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets records that have been modified since the specified time.
    /// </summary>
    Task<IReadOnlyList<PleasanterRecord>> GetChangedRecordsAsync(
        string connectionString,
        DbmsType dbmsType,
        long siteId,
        ReferenceType referenceType,
        DateTime lastSyncTime,
        int syncUserId,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets records that have been deleted since the specified time.
    /// </summary>
    Task<IReadOnlyList<PleasanterRecord>> GetDeletedRecordsAsync(
        string connectionString,
        DbmsType dbmsType,
        long siteId,
        ReferenceType referenceType,
        DateTime lastSyncTime,
        IReadOnlyList<string> syncKeyColumns,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a record in the target by sync key values.
    /// </summary>
    Task<PleasanterRecord?> FindRecordBySyncKeyAsync(
        string connectionString,
        DbmsType dbmsType,
        long siteId,
        ReferenceType referenceType,
        IReadOnlyList<string> syncKeyColumns,
        IReadOnlyList<object?> syncKeyValues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a record in the target instance.
    /// </summary>
    Task UpsertRecordAsync(
        string connectionString,
        DbmsType dbmsType,
        long targetSiteId,
        ReferenceType referenceType,
        PleasanterRecord sourceRecord,
        PleasanterRecord? existingTarget,
        IReadOnlyList<string> syncColumns,
        int syncUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a record from the target instance (moving it to _deleted table first).
    /// </summary>
    Task DeleteRecordAsync(
        string connectionString,
        DbmsType dbmsType,
        long targetSiteId,
        ReferenceType referenceType,
        PleasanterRecord targetRecord,
        int syncUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests a database connection.
    /// </summary>
    Task<bool> TestConnectionAsync(
        string connectionString,
        DbmsType dbmsType,
        CancellationToken cancellationToken = default);
}
