using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;

namespace VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Pleasanter.SqlGenerators;

/// <summary>
/// PostgreSQL specific SQL generator for Pleasanter operations.
/// </summary>
public class PostgreSqlGenerator : ISqlGenerator
{
    /// <inheritdoc />
    public DbmsType DbmsType => DbmsType.PostgreSql;

    /// <inheritdoc />
    public string ParameterPrefix => "@";

    /// <inheritdoc />
    public string CurrentTimestampFunction => "CURRENT_TIMESTAMP";

    /// <inheritdoc />
    public string QuoteIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        return $"\"{identifier}\"";
    }

    /// <inheritdoc />
    public string GetIdColumnName(string tableName) => tableName switch
    {
        "Results" => "ResultId",
        "Issues" => "IssueId",
        "Wikis" => "WikiId",
        _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Unsupported table name.")
    };

    /// <inheritdoc />
    public string GetReferenceTypeSql()
    {
        return """SELECT "ReferenceType" FROM "Sites" WHERE "SiteId" = @SiteId""";
    }

    /// <inheritdoc />
    public string GetChangedRecordsSql(string tableName, IReadOnlyList<string> columns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(columns);

        var q = QuoteIdentifier(tableName);
        var idCol = QuoteIdentifier(GetIdColumnName(tableName));
        var baseCols = "\"SiteId\", \"Ver\", \"Title\", \"Body\", \"Creator\", \"Updator\", \"CreatedTime\", \"UpdatedTime\"";
        if (tableName == "Wikis")
        {
            baseCols += ", \"Locked\"";
        }

        var selectCols = columns.Count > 0
            ? ", " + string.Join(", ", columns.Select(c => QuoteIdentifier(c)))
            : string.Empty;

        return $"""
            SELECT {idCol} AS "RecordId", {baseCols}{selectCols}
            FROM {q}
            WHERE "SiteId" = @SiteId
              AND "UpdatedTime" > @LastSyncTime
              AND "Updator" <> @SyncUserId
            ORDER BY "UpdatedTime" ASC
            """;
    }

    /// <inheritdoc />
    public string GetDeletedRecordsSql(string tableName, IReadOnlyList<string> syncKeyColumns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(syncKeyColumns);

        var deletedTable = QuoteIdentifier($"{tableName}_deleted");
        var idCol = QuoteIdentifier(GetIdColumnName(tableName));
        var keyCols = syncKeyColumns.Count > 0
            ? ", " + string.Join(", ", syncKeyColumns.Select(c => QuoteIdentifier(c)))
            : string.Empty;

        return $"""
            SELECT {idCol} AS "RecordId", "SiteId"{keyCols}, "UpdatedTime"
            FROM {deletedTable}
            WHERE "SiteId" = @SiteId
              AND "UpdatedTime" > @LastSyncTime
            ORDER BY "UpdatedTime" ASC
            """;
    }

    /// <inheritdoc />
    public string GetFindBySyncKeySql(string tableName, IReadOnlyList<string> syncKeyColumns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(syncKeyColumns);

        var q = QuoteIdentifier(tableName);
        var idCol = QuoteIdentifier(GetIdColumnName(tableName));
        var keyConditions = string.Join(" AND ", syncKeyColumns.Select(c => $"{QuoteIdentifier(c)} = @Key_{c}"));
        var baseCols = "\"SiteId\", \"Ver\", \"Title\", \"Body\", \"Creator\", \"Updator\", \"CreatedTime\", \"UpdatedTime\"";
        if (tableName == "Wikis")
        {
            baseCols += ", \"Locked\"";
        }

        return $"""
            SELECT {idCol} AS "RecordId", {baseCols}
            FROM {q}
            WHERE "SiteId" = @SiteId AND {keyConditions}
            """;
    }

    /// <inheritdoc />
    public string GetInsertRecordSql(string tableName, IReadOnlyList<string> columns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(columns);

        var q = QuoteIdentifier(tableName);
        var idCol = QuoteIdentifier(GetIdColumnName(tableName));
        var allCols = new List<string> { "SiteId", "Title", "Body", "Ver", "Creator", "Updator", "CreatedTime", "UpdatedTime" };
        if (tableName == "Wikis")
        {
            allCols.Add("Locked");
        }

        allCols.AddRange(columns);
        var colList = string.Join(", ", allCols.Select(c => QuoteIdentifier(c)));
        var paramList = string.Join(", ", allCols.Select(c => $"@{c}"));

        return $"""
            INSERT INTO {q} ({colList})
            VALUES ({paramList})
            RETURNING {idCol}
            """;
    }

    /// <inheritdoc />
    public string GetUpdateRecordSql(string tableName, IReadOnlyList<string> columns, IReadOnlyList<string> syncKeyColumns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(syncKeyColumns);

        var q = QuoteIdentifier(tableName);
        var setClauses = new List<string>(columns.Select(c => $"{QuoteIdentifier(c)} = @{c}"))
        {
            "\"Title\" = @Title",
            "\"Body\" = @Body",
            "\"Ver\" = \"Ver\" + 1",
            "\"Updator\" = @SyncUserId",
            $"\"UpdatedTime\" = {CurrentTimestampFunction}"
        };
        if (tableName == "Wikis")
        {
            setClauses.Add("\"Locked\" = @Locked");
        }

        var setStr = string.Join(", ", setClauses);
        var keyConditions = string.Join(" AND ", syncKeyColumns.Select(c => $"{QuoteIdentifier(c)} = @Key_{c}"));

        return $"UPDATE {q} SET {setStr} WHERE \"SiteId\" = @SiteId AND {keyConditions}";
    }

    /// <inheritdoc />
    public string GetInsertItemSql()
    {
        return $"""
            INSERT INTO "Items" ("ReferenceId", "ReferenceType", "SiteId", "Title", "Updator", "UpdatedTime")
            VALUES (@ReferenceId, @ReferenceType, @SiteId, @Title, @SyncUserId, {CurrentTimestampFunction})
            """;
    }

    /// <inheritdoc />
    public string GetUpdateItemSql()
    {
        return $"""
            UPDATE "Items" SET "Title" = @Title, "Updator" = @SyncUserId, "UpdatedTime" = {CurrentTimestampFunction}
            WHERE "ReferenceId" = @ReferenceId AND "ReferenceType" = @ReferenceType AND "SiteId" = @SiteId
            """;
    }

    /// <inheritdoc />
    public string GetCopyToDeletedSql(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        var src = QuoteIdentifier(tableName);
        var dst = QuoteIdentifier($"{tableName}_deleted");
        var idCol = QuoteIdentifier(GetIdColumnName(tableName));

        return $"INSERT INTO {dst} SELECT * FROM {src} WHERE {idCol} = @RecordId AND \"SiteId\" = @SiteId";
    }

    /// <inheritdoc />
    public string GetDeleteRecordSql(string tableName, IReadOnlyList<string> syncKeyColumns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(syncKeyColumns);

        var q = QuoteIdentifier(tableName);
        var idCol = QuoteIdentifier(GetIdColumnName(tableName));
        return $"DELETE FROM {q} WHERE {idCol} = @RecordId AND \"SiteId\" = @SiteId";
    }

    /// <inheritdoc />
    public string GetDeleteItemSql()
    {
        return """DELETE FROM "Items" WHERE "ReferenceId" = @ReferenceId AND "SiteId" = @SiteId""";
    }
}
