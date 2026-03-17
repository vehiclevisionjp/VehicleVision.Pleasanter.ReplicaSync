using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Pleasanter.SqlGenerators;

namespace VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Pleasanter;

/// <summary>
/// ADO.NET implementation of Pleasanter database access.
/// </summary>
public class PleasanterDbAccess : IPleasanterDbAccess
{
    private readonly ILogger<PleasanterDbAccess> _logger;
    private readonly Dictionary<DbmsType, ISqlGenerator> _generators;

    /// <summary>
    /// Initializes a new instance of the <see cref="PleasanterDbAccess"/> class.
    /// </summary>
    public PleasanterDbAccess(ILogger<PleasanterDbAccess> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _generators = new Dictionary<DbmsType, ISqlGenerator>
        {
            [DbmsType.SqlServer] = new SqlServerSqlGenerator(),
            [DbmsType.PostgreSql] = new PostgreSqlGenerator(),
            [DbmsType.MySql] = new MySqlSqlGenerator()
        };
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(
        string connectionString,
        DbmsType dbmsType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        try
        {
            await using var connection = CreateConnection(connectionString, dbmsType);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Connection test succeeded for {DbmsType}", dbmsType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection test failed for {DbmsType}", dbmsType);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<ReferenceType> GetReferenceTypeAsync(
        string connectionString,
        DbmsType dbmsType,
        long siteId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var generator = GetGenerator(dbmsType);
        var sql = generator.GetReferenceTypeSql();

        await using var connection = CreateConnection(connectionString, dbmsType);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@SiteId", siteId);

        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result is null || result is DBNull)
        {
            throw new InvalidOperationException($"Site {siteId} not found.");
        }

        var refTypeStr = result.ToString()!;
        return refTypeStr switch
        {
            "Results" => ReferenceType.Results,
            "Issues" => ReferenceType.Issues,
            "Wikis" => ReferenceType.Wikis,
            _ => throw new InvalidOperationException($"Unsupported reference type: {refTypeStr}")
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PleasanterRecord>> GetChangedRecordsAsync(
        string connectionString,
        DbmsType dbmsType,
        long siteId,
        ReferenceType referenceType,
        DateTime lastSyncTime,
        int syncUserId,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(columns);

        var generator = GetGenerator(dbmsType);
        var tableName = referenceType.ToString();
        var sql = generator.GetChangedRecordsSql(tableName, columns);

        await using var connection = CreateConnection(connectionString, dbmsType);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@SiteId", siteId);
        AddParameter(command, "@LastSyncTime", lastSyncTime);
        AddParameter(command, "@SyncUserId", syncUserId);

        var records = new List<PleasanterRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var record = ReadBaseRecord(reader);
            ReadColumnValues(reader, record, columns);
            records.Add(record);
        }

        _logger.LogDebug(
            "Found {Count} changed records for site {SiteId} since {LastSyncTime}",
            records.Count, siteId, lastSyncTime);

        return records;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PleasanterRecord>> GetDeletedRecordsAsync(
        string connectionString,
        DbmsType dbmsType,
        long siteId,
        ReferenceType referenceType,
        DateTime lastSyncTime,
        IReadOnlyList<string> syncKeyColumns,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(syncKeyColumns);

        var generator = GetGenerator(dbmsType);
        var tableName = referenceType.ToString();
        var sql = generator.GetDeletedRecordsSql(tableName, syncKeyColumns);

        await using var connection = CreateConnection(connectionString, dbmsType);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@SiteId", siteId);
        AddParameter(command, "@LastSyncTime", lastSyncTime);

        var records = new List<PleasanterRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var record = new PleasanterRecord
            {
                RecordId = reader.GetInt64(reader.GetOrdinal("RecordId")),
                SiteId = reader.GetInt64(reader.GetOrdinal("SiteId")),
                UpdatedTime = reader.GetDateTime(reader.GetOrdinal("UpdatedTime")),
                IsDeleted = true
            };

            foreach (var col in syncKeyColumns)
            {
                var ordinal = reader.GetOrdinal(col);
                record.ColumnValues[col] = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
            }

            records.Add(record);
        }

        _logger.LogDebug(
            "Found {Count} deleted records for site {SiteId} since {LastSyncTime}",
            records.Count, siteId, lastSyncTime);

        return records;
    }

    /// <inheritdoc />
    public async Task<PleasanterRecord?> FindRecordBySyncKeyAsync(
        string connectionString,
        DbmsType dbmsType,
        long siteId,
        ReferenceType referenceType,
        IReadOnlyList<string> syncKeyColumns,
        IReadOnlyList<object?> syncKeyValues,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(syncKeyColumns);
        ArgumentNullException.ThrowIfNull(syncKeyValues);

        var generator = GetGenerator(dbmsType);
        var tableName = referenceType.ToString();
        var sql = generator.GetFindBySyncKeySql(tableName, syncKeyColumns);

        await using var connection = CreateConnection(connectionString, dbmsType);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@SiteId", siteId);

        for (var i = 0; i < syncKeyColumns.Count; i++)
        {
            AddParameter(command, $"@Key_{syncKeyColumns[i]}", syncKeyValues[i] ?? DBNull.Value);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return ReadBaseRecord(reader);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task UpsertRecordAsync(
        string connectionString,
        DbmsType dbmsType,
        long targetSiteId,
        ReferenceType referenceType,
        PleasanterRecord sourceRecord,
        PleasanterRecord? existingTarget,
        IReadOnlyList<string> syncColumns,
        int syncUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(sourceRecord);
        ArgumentNullException.ThrowIfNull(syncColumns);

        var generator = GetGenerator(dbmsType);
        var tableName = referenceType.ToString();

        await using var connection = CreateConnection(connectionString, dbmsType);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (existingTarget is null)
            {
                await InsertRecordAsync(
                    connection, transaction, generator, tableName,
                    targetSiteId, referenceType, sourceRecord, syncColumns, syncUserId,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await UpdateRecordAsync(
                    connection, transaction, generator, tableName,
                    targetSiteId, sourceRecord, syncColumns, syncUserId,
                    cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteRecordAsync(
        string connectionString,
        DbmsType dbmsType,
        long targetSiteId,
        ReferenceType referenceType,
        PleasanterRecord targetRecord,
        int syncUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(targetRecord);

        var generator = GetGenerator(dbmsType);
        var tableName = referenceType.ToString();

        await using var connection = CreateConnection(connectionString, dbmsType);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Copy to deleted table
            await using (var copyCmd = connection.CreateCommand())
            {
                copyCmd.Transaction = transaction;
                copyCmd.CommandText = generator.GetCopyToDeletedSql(tableName);
                AddParameter(copyCmd, "@RecordId", targetRecord.RecordId);
                AddParameter(copyCmd, "@SiteId", targetSiteId);
                await copyCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            // Delete from main table
            await using (var deleteCmd = connection.CreateCommand())
            {
                deleteCmd.Transaction = transaction;
                deleteCmd.CommandText = generator.GetDeleteRecordSql(tableName, []);
                AddParameter(deleteCmd, "@RecordId", targetRecord.RecordId);
                AddParameter(deleteCmd, "@SiteId", targetSiteId);
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            // Delete from Items table
            await using (var itemCmd = connection.CreateCommand())
            {
                itemCmd.Transaction = transaction;
                itemCmd.CommandText = generator.GetDeleteItemSql();
                AddParameter(itemCmd, "@ReferenceId", targetRecord.RecordId);
                AddParameter(itemCmd, "@SiteId", targetSiteId);
                await itemCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug(
                "Deleted record {RecordId} from site {SiteId}",
                targetRecord.RecordId, targetSiteId);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private ISqlGenerator GetGenerator(DbmsType dbmsType)
    {
        if (!_generators.TryGetValue(dbmsType, out var generator))
        {
            throw new ArgumentOutOfRangeException(nameof(dbmsType), dbmsType, "Unsupported DBMS type.");
        }

        return generator;
    }

    private static DbConnection CreateConnection(string connectionString, DbmsType dbmsType)
    {
        return dbmsType switch
        {
            DbmsType.SqlServer => new SqlConnection(connectionString),
            DbmsType.PostgreSql => new NpgsqlConnection(connectionString),
            DbmsType.MySql => new MySqlConnection(connectionString),
            _ => throw new ArgumentOutOfRangeException(nameof(dbmsType), dbmsType, "Unsupported DBMS type.")
        };
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static PleasanterRecord ReadBaseRecord(DbDataReader reader)
    {
        var record = new PleasanterRecord
        {
            RecordId = reader.GetInt64(reader.GetOrdinal("RecordId")),
            SiteId = reader.GetInt64(reader.GetOrdinal("SiteId")),
            Ver = reader.GetInt32(reader.GetOrdinal("Ver")),
            Title = GetStringOrEmpty(reader, "Title"),
            Body = GetStringOrEmpty(reader, "Body"),
            Creator = reader.GetInt32(reader.GetOrdinal("Creator")),
            Updator = reader.GetInt32(reader.GetOrdinal("Updator")),
            CreatedTime = reader.GetDateTime(reader.GetOrdinal("CreatedTime")),
            UpdatedTime = reader.GetDateTime(reader.GetOrdinal("UpdatedTime"))
        };

        if (HasColumn(reader, "Locked"))
        {
            var ordinal = reader.GetOrdinal("Locked");
            record.Locked = !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
        }

        return record;
    }

    private static bool HasColumn(DbDataReader reader, string columnName)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetStringOrEmpty(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static void ReadColumnValues(DbDataReader reader, PleasanterRecord record, IReadOnlyList<string> columns)
    {
        foreach (var col in columns)
        {
            var ordinal = reader.GetOrdinal(col);
            record.ColumnValues[col] = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
        }
    }

    private async Task InsertRecordAsync(
        DbConnection connection,
        DbTransaction transaction,
        ISqlGenerator generator,
        string tableName,
        long targetSiteId,
        ReferenceType referenceType,
        PleasanterRecord sourceRecord,
        IReadOnlyList<string> syncColumns,
        int syncUserId,
        CancellationToken cancellationToken)
    {
        long newRecordId;

        // Insert into main table
        await using (var insertCmd = connection.CreateCommand())
        {
            insertCmd.Transaction = transaction;
            insertCmd.CommandText = generator.GetInsertRecordSql(tableName, syncColumns);
            AddParameter(insertCmd, "@SiteId", targetSiteId);
            AddParameter(insertCmd, "@Title", sourceRecord.Title);
            AddParameter(insertCmd, "@Body", sourceRecord.Body);
            AddParameter(insertCmd, "@Ver", 1);
            AddParameter(insertCmd, "@Creator", syncUserId);
            AddParameter(insertCmd, "@Updator", syncUserId);
            AddParameter(insertCmd, "@CreatedTime", DateTime.UtcNow);
            AddParameter(insertCmd, "@UpdatedTime", DateTime.UtcNow);

            if (tableName == "Wikis")
            {
                AddParameter(insertCmd, "@Locked", sourceRecord.Locked);
            }

            foreach (var col in syncColumns)
            {
                AddParameter(insertCmd, $"@{col}", sourceRecord.GetColumnValue(col) ?? DBNull.Value);
            }

            var result = await insertCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            newRecordId = Convert.ToInt64(result, System.Globalization.CultureInfo.InvariantCulture);
        }

        // Insert into Items table
        await using (var itemCmd = connection.CreateCommand())
        {
            itemCmd.Transaction = transaction;
            itemCmd.CommandText = generator.GetInsertItemSql();
            AddParameter(itemCmd, "@ReferenceId", newRecordId);
            AddParameter(itemCmd, "@ReferenceType", referenceType.ToString());
            AddParameter(itemCmd, "@SiteId", targetSiteId);
            AddParameter(itemCmd, "@Title", sourceRecord.Title);
            AddParameter(itemCmd, "@SyncUserId", syncUserId);
            await itemCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        _logger.LogDebug(
            "Inserted record {RecordId} into site {SiteId}",
            newRecordId, targetSiteId);
    }

    private async Task UpdateRecordAsync(
        DbConnection connection,
        DbTransaction transaction,
        ISqlGenerator generator,
        string tableName,
        long targetSiteId,
        PleasanterRecord sourceRecord,
        IReadOnlyList<string> syncColumns,
        int syncUserId,
        CancellationToken cancellationToken)
    {
        // Build sync key columns from the source record's column values
        var syncKeyColumns = sourceRecord.ColumnValues.Keys
            .Where(k => syncColumns.Contains(k))
            .ToList();

        // Update main table
        await using (var updateCmd = connection.CreateCommand())
        {
            updateCmd.Transaction = transaction;
            updateCmd.CommandText = generator.GetUpdateRecordSql(tableName, syncColumns, syncKeyColumns);
            AddParameter(updateCmd, "@SiteId", targetSiteId);
            AddParameter(updateCmd, "@Title", sourceRecord.Title);
            AddParameter(updateCmd, "@Body", sourceRecord.Body);
            AddParameter(updateCmd, "@SyncUserId", syncUserId);

            if (tableName == "Wikis")
            {
                AddParameter(updateCmd, "@Locked", sourceRecord.Locked);
            }

            foreach (var col in syncColumns)
            {
                AddParameter(updateCmd, $"@{col}", sourceRecord.GetColumnValue(col) ?? DBNull.Value);
            }

            foreach (var col in syncKeyColumns)
            {
                AddParameter(updateCmd, $"@Key_{col}", sourceRecord.GetColumnValue(col) ?? DBNull.Value);
            }

            await updateCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        // Update Items table
        await using (var itemCmd = connection.CreateCommand())
        {
            itemCmd.Transaction = transaction;
            itemCmd.CommandText = generator.GetUpdateItemSql();
            AddParameter(itemCmd, "@ReferenceId", sourceRecord.RecordId);
            AddParameter(itemCmd, "@ReferenceType", tableName);
            AddParameter(itemCmd, "@SiteId", targetSiteId);
            AddParameter(itemCmd, "@Title", sourceRecord.Title);
            AddParameter(itemCmd, "@SyncUserId", syncUserId);
            await itemCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        _logger.LogDebug(
            "Updated record in site {SiteId} from source record {RecordId}",
            targetSiteId, sourceRecord.RecordId);
    }
}
