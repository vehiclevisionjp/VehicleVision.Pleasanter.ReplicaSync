using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Pleasanter.SqlGenerators;

namespace VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Tests.Pleasanter.SqlGenerators;

/// <summary>
/// Tests for all ISqlGenerator implementations (SqlServer, PostgreSQL, MySQL).
/// Uses Theory + MemberData to run each test against all three generators.
/// </summary>
public class SqlGeneratorTests
{
    public static TheoryData<ISqlGenerator> AllGenerators => new()
    {
        new SqlServerSqlGenerator(),
        new PostgreSqlGenerator(),
        new MySqlSqlGenerator(),
    };

    // ── DbmsType / Properties ──

    [Fact]
    public void SqlServerGeneratorShouldReturnCorrectDbmsType()
    {
        var generator = new SqlServerSqlGenerator();
        Assert.Equal(DbmsType.SqlServer, generator.DbmsType);
    }

    [Fact]
    public void PostgreSqlGeneratorShouldReturnCorrectDbmsType()
    {
        var generator = new PostgreSqlGenerator();
        Assert.Equal(DbmsType.PostgreSql, generator.DbmsType);
    }

    [Fact]
    public void MySqlGeneratorShouldReturnCorrectDbmsType()
    {
        var generator = new MySqlSqlGenerator();
        Assert.Equal(DbmsType.MySql, generator.DbmsType);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void ParameterPrefixShouldBeAtSign(ISqlGenerator generator)
    {
        Assert.Equal("@", generator.ParameterPrefix);
    }

    [Fact]
    public void CurrentTimestampFunctionShouldBeDbmsSpecific()
    {
        Assert.Equal("GETUTCDATE()", new SqlServerSqlGenerator().CurrentTimestampFunction);
        Assert.Equal("CURRENT_TIMESTAMP", new PostgreSqlGenerator().CurrentTimestampFunction);
        Assert.Equal("NOW()", new MySqlSqlGenerator().CurrentTimestampFunction);
    }

    // ── QuoteIdentifier ──

    [Fact]
    public void SqlServerQuoteIdentifierShouldUseBrackets()
    {
        var generator = new SqlServerSqlGenerator();
        Assert.Equal("[MyTable]", generator.QuoteIdentifier("MyTable"));
    }

    [Fact]
    public void PostgreSqlQuoteIdentifierShouldUseDoubleQuotes()
    {
        var generator = new PostgreSqlGenerator();
        Assert.Equal("\"MyTable\"", generator.QuoteIdentifier("MyTable"));
    }

    [Fact]
    public void MySqlQuoteIdentifierShouldUseBackticks()
    {
        var generator = new MySqlSqlGenerator();
        Assert.Equal("`MyTable`", generator.QuoteIdentifier("MyTable"));
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void QuoteIdentifierShouldThrowWhenNullOrWhiteSpace(ISqlGenerator generator)
    {
        Assert.ThrowsAny<ArgumentException>(() => generator.QuoteIdentifier(null!));
        Assert.Throws<ArgumentException>(() => generator.QuoteIdentifier(""));
        Assert.Throws<ArgumentException>(() => generator.QuoteIdentifier("   "));
    }

    // ── GetIdColumnName ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetIdColumnNameShouldReturnResultId(ISqlGenerator generator)
    {
        Assert.Equal("ResultId", generator.GetIdColumnName("Results"));
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetIdColumnNameShouldReturnIssueId(ISqlGenerator generator)
    {
        Assert.Equal("IssueId", generator.GetIdColumnName("Issues"));
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetIdColumnNameShouldReturnWikiId(ISqlGenerator generator)
    {
        Assert.Equal("WikiId", generator.GetIdColumnName("Wikis"));
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetIdColumnNameShouldThrowForUnsupportedTable(ISqlGenerator generator)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GetIdColumnName("Unknown"));
    }

    // ── GetReferenceTypeSql ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetReferenceTypeSqlShouldContainSiteIdParameter(ISqlGenerator generator)
    {
        var sql = generator.GetReferenceTypeSql();
        Assert.Contains("@SiteId", sql);
        Assert.Contains("Sites", sql);
        Assert.Contains("ReferenceType", sql);
    }

    // ── GetChangedRecordsSql ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetChangedRecordsSqlShouldContainResultIdForResults(ISqlGenerator generator)
    {
        var sql = generator.GetChangedRecordsSql("Results", ["ClassA"]);
        Assert.Contains("ResultId", sql);
        Assert.Contains("RecordId", sql);
        Assert.Contains("@SiteId", sql);
        Assert.Contains("@LastSyncTime", sql);
        Assert.Contains("@SyncUserId", sql);
        Assert.Contains("ClassA", sql);
        Assert.DoesNotContain("Locked", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetChangedRecordsSqlShouldContainIssueIdForIssues(ISqlGenerator generator)
    {
        var sql = generator.GetChangedRecordsSql("Issues", ["NumA"]);
        Assert.Contains("IssueId", sql);
        Assert.Contains("NumA", sql);
        Assert.DoesNotContain("Locked", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetChangedRecordsSqlShouldContainWikiIdAndLockedForWikis(ISqlGenerator generator)
    {
        var sql = generator.GetChangedRecordsSql("Wikis", []);
        Assert.Contains("WikiId", sql);
        Assert.Contains("Locked", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetChangedRecordsSqlShouldHandleEmptyColumnList(ISqlGenerator generator)
    {
        var sql = generator.GetChangedRecordsSql("Results", []);
        Assert.Contains("RecordId", sql);
        Assert.Contains("Title", sql);
        Assert.Contains("Body", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetChangedRecordsSqlShouldThrowWhenTableNameIsNull(ISqlGenerator generator)
    {
        Assert.ThrowsAny<ArgumentException>(() => generator.GetChangedRecordsSql(null!, []));
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetChangedRecordsSqlShouldThrowWhenColumnsIsNull(ISqlGenerator generator)
    {
        Assert.Throws<ArgumentNullException>(() => generator.GetChangedRecordsSql("Results", null!));
    }

    // ── GetDeletedRecordsSql ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetDeletedRecordsSqlShouldReferenceDeletedTable(ISqlGenerator generator)
    {
        var sql = generator.GetDeletedRecordsSql("Results", ["ClassA"]);
        Assert.Contains("Results_deleted", sql);
        Assert.Contains("ResultId", sql);
        Assert.Contains("ClassA", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetDeletedRecordsSqlShouldUseWikiIdForWikis(ISqlGenerator generator)
    {
        var sql = generator.GetDeletedRecordsSql("Wikis", []);
        Assert.Contains("Wikis_deleted", sql);
        Assert.Contains("WikiId", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetDeletedRecordsSqlShouldHandleEmptySyncKeyColumns(ISqlGenerator generator)
    {
        var sql = generator.GetDeletedRecordsSql("Results", []);
        Assert.Contains("RecordId", sql);
        Assert.Contains("@SiteId", sql);
    }

    // ── GetFindBySyncKeySql ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetFindBySyncKeySqlShouldContainKeyConditions(ISqlGenerator generator)
    {
        var sql = generator.GetFindBySyncKeySql("Results", ["ClassA", "ClassB"]);
        Assert.Contains("@Key_ClassA", sql);
        Assert.Contains("@Key_ClassB", sql);
        Assert.Contains("ResultId", sql);
        Assert.DoesNotContain("Locked", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetFindBySyncKeySqlShouldIncludeLockedForWikis(ISqlGenerator generator)
    {
        var sql = generator.GetFindBySyncKeySql("Wikis", ["Title"]);
        Assert.Contains("WikiId", sql);
        Assert.Contains("Locked", sql);
        Assert.Contains("@Key_Title", sql);
    }

    // ── GetInsertRecordSql ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetInsertRecordSqlShouldContainBasicColumns(ISqlGenerator generator)
    {
        var sql = generator.GetInsertRecordSql("Results", ["ClassA"]);
        Assert.Contains("SiteId", sql);
        Assert.Contains("Title", sql);
        Assert.Contains("Body", sql);
        Assert.Contains("Ver", sql);
        Assert.Contains("Creator", sql);
        Assert.Contains("Updator", sql);
        Assert.Contains("ClassA", sql);
        Assert.DoesNotContain("Locked", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetInsertRecordSqlShouldIncludeLockedForWikis(ISqlGenerator generator)
    {
        var sql = generator.GetInsertRecordSql("Wikis", []);
        Assert.Contains("Locked", sql);
        Assert.Contains("@Locked", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetInsertRecordSqlShouldHandleEmptyColumnList(ISqlGenerator generator)
    {
        var sql = generator.GetInsertRecordSql("Results", []);
        Assert.Contains("SiteId", sql);
        Assert.Contains("Title", sql);
    }

    // ── GetUpdateRecordSql ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetUpdateRecordSqlShouldContainSetAndWhereClause(ISqlGenerator generator)
    {
        var sql = generator.GetUpdateRecordSql("Results", ["ClassA"], ["ClassA"]);
        Assert.Contains("UPDATE", sql);
        Assert.Contains("@Title", sql);
        Assert.Contains("@Body", sql);
        Assert.Contains("@SyncUserId", sql);
        Assert.Contains("@Key_ClassA", sql);
        Assert.DoesNotContain("Locked", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetUpdateRecordSqlShouldIncludeLockedForWikis(ISqlGenerator generator)
    {
        var sql = generator.GetUpdateRecordSql("Wikis", [], ["Title"]);
        Assert.Contains("@Locked", sql);
        Assert.Contains("Locked", sql);
    }

    // ── GetInsertItemSql / GetUpdateItemSql / GetDeleteItemSql ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetInsertItemSqlShouldContainRequiredParameters(ISqlGenerator generator)
    {
        var sql = generator.GetInsertItemSql();
        Assert.Contains("Items", sql);
        Assert.Contains("@ReferenceId", sql);
        Assert.Contains("@ReferenceType", sql);
        Assert.Contains("@SiteId", sql);
        Assert.Contains("@Title", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetUpdateItemSqlShouldContainRequiredParameters(ISqlGenerator generator)
    {
        var sql = generator.GetUpdateItemSql();
        Assert.Contains("UPDATE", sql);
        Assert.Contains("Items", sql);
        Assert.Contains("@ReferenceId", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetDeleteItemSqlShouldContainRequiredParameters(ISqlGenerator generator)
    {
        var sql = generator.GetDeleteItemSql();
        Assert.Contains("DELETE", sql);
        Assert.Contains("Items", sql);
        Assert.Contains("@ReferenceId", sql);
    }

    // ── GetCopyToDeletedSql ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetCopyToDeletedSqlShouldReferenceDeletedTable(ISqlGenerator generator)
    {
        var sql = generator.GetCopyToDeletedSql("Results");
        Assert.Contains("Results_deleted", sql);
        Assert.Contains("ResultId", sql);
        Assert.Contains("@RecordId", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetCopyToDeletedSqlShouldUseWikiIdForWikis(ISqlGenerator generator)
    {
        var sql = generator.GetCopyToDeletedSql("Wikis");
        Assert.Contains("Wikis_deleted", sql);
        Assert.Contains("WikiId", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetCopyToDeletedSqlShouldThrowWhenTableNameIsNull(ISqlGenerator generator)
    {
        Assert.ThrowsAny<ArgumentException>(() => generator.GetCopyToDeletedSql(null!));
    }

    // ── GetDeleteRecordSql ──

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetDeleteRecordSqlShouldContainIdColumnAndRecordId(ISqlGenerator generator)
    {
        var sql = generator.GetDeleteRecordSql("Issues", []);
        Assert.Contains("DELETE", sql);
        Assert.Contains("IssueId", sql);
        Assert.Contains("@RecordId", sql);
        Assert.Contains("@SiteId", sql);
    }

    [Theory]
    [MemberData(nameof(AllGenerators))]
    public void GetDeleteRecordSqlShouldUseWikiIdForWikis(ISqlGenerator generator)
    {
        var sql = generator.GetDeleteRecordSql("Wikis", []);
        Assert.Contains("WikiId", sql);
    }
}
