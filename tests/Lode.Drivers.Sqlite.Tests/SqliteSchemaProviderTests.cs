using Lode.Core;
using Lode.Core.ValueTypes;
using Lode.Drivers.Sqlite;
using Lode.Tests.Common;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Driver)]
[Category(TestCategories.Sqlite)]
public class SqliteSchemaProviderTests
{
    private SqliteDbConnection _connection;

    [SetUp]
    public async Task Setup()
    {
        var driver = new SqliteDriver();
        var opt = new DbConnectionOptions() { FilePath = ":memory:" };
        var result = await driver.OpenConnectionAsync(opt);
        _connection = (SqliteDbConnection)result.Data;

        await _connection.Query.ExecuteNonQueryAsync("""
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Age INTEGER,
                Balance DECIMAL NOT NULL
            );
            """);

        await _connection.Query.ExecuteNonQueryAsync("""
            CREATE TABLE Products (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Price DECIMAL NOT NULL
            );
            """);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task GetTableNamesAsync_ShouldReturnAllTables()
    {
        var result = await _connection.Schema.GetTableNamesAsync();
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Contains.Item("Users"));
        Assert.That(result.Data, Contains.Item("Products"));
    }

    [Test]
    public async Task GetTableNamesAsync_ShouldReturnTablesInAlphabeticalOrder()
    {
        var result = await _connection.Schema.GetTableNamesAsync();
        var names = result.Data.ToList();
        Assert.That(names, Is.EqualTo(names.OrderBy(n => n).ToList()));
    }

    [Test]
    public async Task GetTableNamesAsync_WithNoTables_ShouldReturnEmptyList()
    {
        await _connection.Query.ExecuteNonQueryAsync("DROP TABLE Users;");
        await _connection.Query.ExecuteNonQueryAsync("DROP TABLE Products;");

        var result = await _connection.Schema.GetTableNamesAsync();
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetTableDefinitionAsync_ShouldReturnCorrectColumnCount()
    {
        var result = await _connection.Schema.GetTableDefinitionAsync("Users");
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data.Columns.Count, Is.EqualTo(4));
    }

    [Test]
    public async Task GetTableDefinitionAsync_ShouldReturnCorrectColumnNames()
    {
        var result = await _connection.Schema.GetTableDefinitionAsync("Users");
        var names = result.Data.Columns.Select(c => c.Name).ToList();
        Assert.That(names, Is.EqualTo(new[] { "Id", "Name", "Age", "Balance" }));
    }

    [Test]
    public async Task GetTableDefinitionAsync_ShouldReturnCorrectColumnTypes()
    {
        var result = await _connection.Schema.GetTableDefinitionAsync("Users");
        var columns = result.Data.Columns.ToList();
        Assert.That(columns[0].Type, Is.EqualTo(CanonicalType.Int));
        Assert.That(columns[1].Type, Is.EqualTo(CanonicalType.String));
        Assert.That(columns[2].Type, Is.EqualTo(CanonicalType.Int));
        Assert.That(columns[3].Type, Is.EqualTo(CanonicalType.Decimal));
    }

    [Test]
    public async Task GetTableDefinitionAsync_ShouldCorrectlyMapPrimaryKeyFlag()
    {
        var result = await _connection.Schema.GetTableDefinitionAsync("Users");
        var columns = result.Data.Columns.ToList();
        Assert.That(columns[0].Flags.HasFlag(ColumnFlags.PrimaryKey), Is.True);
        Assert.That(columns[1].Flags.HasFlag(ColumnFlags.PrimaryKey), Is.False);
    }

    [Test]
    public async Task GetTableDefinitionAsync_ShouldCorrectlyMapNullableFlag()
    {
        var result = await _connection.Schema.GetTableDefinitionAsync("Users");
        var columns = result.Data.Columns.ToList();
        Assert.That(columns[2].Flags.HasFlag(ColumnFlags.Nullable), Is.True);
        Assert.That(columns[1].Flags.HasFlag(ColumnFlags.NotNull), Is.True);
    }

    [Test]
    public async Task GetTableDefinitionAsync_WithNonExistentTable_ShouldReturnFailure()
    {
        var result = await _connection.Schema.GetTableDefinitionAsync("NonExistentTable");
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data.Columns, Is.Empty);
    }

    [Test]
    public async Task GetSchemaAsync_ShouldReturnSuccess()
    {
        var result = await _connection.Schema.GetSchemaAsync();
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetSchemaAsync_ShouldContainAllTableDefinitions()
    {
        var result = await _connection.Schema.GetSchemaAsync();
        Assert.That(result.Data, Does.Contain("Users"));
        Assert.That(result.Data, Does.Contain("Products"));
    }

    [Test]
    public async Task GetSchemaAsync_ShouldContainCreateTableStatements()
    {
        var result = await _connection.Schema.GetSchemaAsync();
        Assert.That(result.Data, Does.Contain("CREATE TABLE"));
    }

    [Test]
    public async Task GetSchemaAsync_WithNoTables_ShouldReturnEmptyString()
    {
        await _connection.Query.ExecuteNonQueryAsync("DROP TABLE Users;");
        await _connection.Query.ExecuteNonQueryAsync("DROP TABLE Products;");

        var result = await _connection.Schema.GetSchemaAsync();
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Empty);
    }
}