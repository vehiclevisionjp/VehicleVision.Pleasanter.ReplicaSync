using ReplicaSync.Core.Models;

namespace ReplicaSync.Core.Services;

/// <summary>
/// Applies sync rules to filter columns and control direction.
/// </summary>
public static class SyncRuleEngine
{
    /// <summary>
    /// Gets the list of columns to sync for a specific target, taking into account
    /// the definition's include/exclude lists and the target's override excludes.
    /// </summary>
    public static IReadOnlyList<string> GetEffectiveColumns(
        SyncDefinition definition,
        SyncTargetMapping targetMapping)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(targetMapping);

        var allPleasanterColumns = GetAllPleasanterDataColumns();
        var includeList = definition.GetIncludeColumnList();
        var excludeList = definition.GetExcludeColumnList();

        // Start with included columns, or all columns if include list is empty
        IEnumerable<string> effectiveColumns = includeList.Count > 0
            ? includeList
            : allPleasanterColumns;

        // Apply definition-level excludes
        if (excludeList.Count > 0)
        {
            var excludeSet = new HashSet<string>(excludeList, StringComparer.OrdinalIgnoreCase);
            effectiveColumns = effectiveColumns.Where(c => !excludeSet.Contains(c));
        }

        // Apply target-level excludes
        if (!string.IsNullOrWhiteSpace(targetMapping.TargetExcludeColumns))
        {
            var targetExcludes = targetMapping.TargetExcludeColumns
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var targetExcludeSet = new HashSet<string>(targetExcludes, StringComparer.OrdinalIgnoreCase);
            effectiveColumns = effectiveColumns.Where(c => !targetExcludeSet.Contains(c));
        }

        return effectiveColumns.ToList().AsReadOnly();
    }

    /// <summary>
    /// Filters source record columns to only include the effective columns.
    /// </summary>
    public static PleasanterRecord FilterRecordColumns(
        PleasanterRecord sourceRecord,
        IReadOnlyList<string> effectiveColumns)
    {
        ArgumentNullException.ThrowIfNull(sourceRecord);
        ArgumentNullException.ThrowIfNull(effectiveColumns);

        var effectiveSet = new HashSet<string>(effectiveColumns, StringComparer.OrdinalIgnoreCase);
        var filteredRecord = new PleasanterRecord
        {
            RecordId = sourceRecord.RecordId,
            SiteId = sourceRecord.SiteId,
            Ver = sourceRecord.Ver,
            Title = effectiveSet.Contains("Title") ? sourceRecord.Title : string.Empty,
            Body = effectiveSet.Contains("Body") ? sourceRecord.Body : string.Empty,
            Creator = sourceRecord.Creator,
            Updator = sourceRecord.Updator,
            CreatedTime = sourceRecord.CreatedTime,
            UpdatedTime = sourceRecord.UpdatedTime,
            IsDeleted = sourceRecord.IsDeleted,
            ColumnValues = new Dictionary<string, object?>(
                sourceRecord.ColumnValues
                    .Where(kv => effectiveSet.Contains(kv.Key)))
        };

        return filteredRecord;
    }

    /// <summary>
    /// Gets all standard Pleasanter data columns (ClassA-ClassZ, NumA-NumZ, DateA-DateZ, DescriptionA-DescriptionZ).
    /// </summary>
    public static IReadOnlyList<string> GetAllPleasanterDataColumns()
    {
        var columns = new List<string> { "Title", "Body" };

        for (var c = 'A'; c <= 'Z'; c++)
        {
            columns.Add($"Class{c}");
        }

        for (var c = 'A'; c <= 'Z'; c++)
        {
            columns.Add($"Num{c}");
        }

        for (var c = 'A'; c <= 'Z'; c++)
        {
            columns.Add($"Date{c}");
        }

        for (var c = 'A'; c <= 'Z'; c++)
        {
            columns.Add($"Description{c}");
        }

        return columns.AsReadOnly();
    }
}
