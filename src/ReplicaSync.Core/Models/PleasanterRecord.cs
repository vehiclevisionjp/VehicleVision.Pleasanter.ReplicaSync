namespace ReplicaSync.Core.Models;

/// <summary>
/// Represents a record from a Pleasanter Results or Issues table.
/// </summary>
public class PleasanterRecord
{
    /// <summary>Gets or sets the record ID (ResultId or IssueId).</summary>
    public long RecordId { get; set; }

    /// <summary>Gets or sets the site ID.</summary>
    public long SiteId { get; set; }

    /// <summary>Gets or sets the version number.</summary>
    public int Ver { get; set; }

    /// <summary>Gets or sets the title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the body.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Gets or sets the creator user ID.</summary>
    public int Creator { get; set; }

    /// <summary>Gets or sets the last updater user ID.</summary>
    public int Updator { get; set; }

    /// <summary>Gets or sets when the record was created.</summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>Gets or sets when the record was last updated.</summary>
    public DateTime UpdatedTime { get; set; }

    /// <summary>Gets or sets the locked state (Wikis only).</summary>
    public bool Locked { get; set; }

    /// <summary>Gets or sets the column values (ClassA-ClassZ, NumA-NumZ, DateA-DateZ, DescriptionA-DescriptionZ).</summary>
    public Dictionary<string, object?> ColumnValues { get; set; } = new();

    /// <summary>
    /// Gets the value of a specific column.
    /// </summary>
    public object? GetColumnValue(string columnName)
    {
        return ColumnValues.TryGetValue(columnName, out var value) ? value : null;
    }

    /// <summary>
    /// Gets whether the record was deleted (exists in _deleted table).
    /// </summary>
    public bool IsDeleted { get; set; }
}
