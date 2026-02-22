using Lode.Core;
using Lode.Tests.Common;

namespace Lode.Drivers.Sqlite.Tests;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Driver)]
[Category(TestCategories.Sqlite)]
public sealed class SqliteImportExportIntegrationTests
{
    private SqliteDriver _driver;
    private DbConnectionOptions _options;

    [SetUp]
    public void Setup()
    {
        _driver = new();
        _options = new()
        {
            FilePath = ":memory:"
        };
    }

    [Test]
    public async Task ExportThenImportAsync_ShouldStreamBetweenConnections()
    {
        var sourceResult = await _driver.OpenConnectionAsync(_options);
        await using var source = sourceResult.Data;

        await source.Query.ExecuteNonQueryAsync("""
            CREATE TABLE users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                age INTEGER
            );
        """);

        await source.Query.ExecuteNonQueryAsync("""
            INSERT INTO users (name, age) VALUES
            ('alice', 20),
            ('bob', 25),
            ('charlie', 30);
        """);

        var schemaResult = await source.Schema.GetTableDefinitionAsync("users");
        Assert.That(schemaResult.IsSuccess, Is.True);
        var schema = schemaResult.Data;

        var rows = source.Exporter.ExportAsync("users");

        var destResult = await _driver.OpenConnectionAsync(_options);
        await using var dest = destResult.Data;

        await dest.Importer.ImportAsync(rows, schema);

        var count = await dest.Query.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM users");

        Assert.That(count.IsSuccess, Is.True);
        Assert.That(count.Data, Is.EqualTo(3));

        var result = await dest.Query.ExecuteQueryAsync("""
            SELECT name, age
            FROM users
            ORDER BY id
        """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data.Rows.Count, Is.EqualTo(3));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Data.Rows[0][0], Is.EqualTo("alice"));
            Assert.That(result.Data.Rows[1][0], Is.EqualTo("bob"));
            Assert.That(result.Data.Rows[2][0], Is.EqualTo("charlie"));
            Assert.That(result.Data.Rows[0][1], Is.EqualTo(20L));
            Assert.That(result.Data.Rows[1][1], Is.EqualTo(25L));
            Assert.That(result.Data.Rows[2][1], Is.EqualTo(30L));
        }
    }
}