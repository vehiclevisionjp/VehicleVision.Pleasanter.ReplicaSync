using ReplicaSync.Core.Enums;

namespace ReplicaSync.Infrastructure.Pleasanter.SqlGenerators;

/// <summary>
/// Interface for generating RDBMS-specific SQL for Pleasanter operations.
/// </summary>
public interface ISqlGenerator
{
    /// <summary>Gets the DBMS type this generator supports.</summary>
    DbmsType DbmsType { get; }

    /// <summary>Gets the parameter prefix (@ for SQL Server/PostgreSQL/MySQL).</summary>
    string ParameterPrefix { get; }

    /// <summary>Gets the current timestamp function.</summary>
    string CurrentTimestampFunction { get; }

    /// <summary>Quotes an identifier (table name, column name).</summary>
    string QuoteIdentifier(string identifier);

    /// <summary>Gets SQL to determine the reference type for a site.</summary>
    string GetReferenceTypeSql();

    /// <summary>Gets SQL to select changed records.</summary>
    string GetChangedRecordsSql(string tableName, IReadOnlyList<string> columns);

    /// <summary>Gets SQL to select deleted records.</summary>
    string GetDeletedRecordsSql(string tableName, IReadOnlyList<string> syncKeyColumns);

    /// <summary>Gets SQL to find a record by sync key.</summary>
    string GetFindBySyncKeySql(string tableName, IReadOnlyList<string> syncKeyColumns);

    /// <summary>Gets SQL to insert a new record, returning the new ID.</summary>
    string GetInsertRecordSql(string tableName, IReadOnlyList<string> columns);

    /// <summary>Gets SQL to update an existing record.</summary>
    string GetUpdateRecordSql(string tableName, IReadOnlyList<string> columns, IReadOnlyList<string> syncKeyColumns);

    /// <summary>Gets SQL to insert an Items table record.</summary>
    string GetInsertItemSql();

    /// <summary>Gets SQL to update an Items table record.</summary>
    string GetUpdateItemSql();

    /// <summary>Gets SQL to copy a record to the _deleted table.</summary>
    string GetCopyToDeletedSql(string tableName);

    /// <summary>Gets SQL to delete a record from the main table.</summary>
    string GetDeleteRecordSql(string tableName, IReadOnlyList<string> syncKeyColumns);

    /// <summary>Gets SQL to delete an Items record.</summary>
    string GetDeleteItemSql();
}
