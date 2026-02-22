using Lode.Core;
using Lode.Core.ValueTypes;
using Lode.Tests.Common;

namespace Lode.Drivers.Sqlite.Tests;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Driver)]
[Category(TestCategories.Sqlite)]
public class SqliteQueryExecutorTests
{
    private SqliteDbConnection _connection;

    [SetUp]
    public async Task Setup()
    {
        var driver = new SqliteDriver();
        var opt = new DbConnectionOptions() { FilePath = ":memory:" };
        var result = await driver.OpenConnectionAsync(opt);
        _connection = (SqliteDbConnection)result.Data;
    }

    [TearDown]
    public async Task TearDown()
    {
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task ExecuteNonQueryAsync_WithValidQuery_ShouldExecuteAndReturnSuccess()
    {
        var createTableSql = "CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);";
        var insertSql = "INSERT INTO Users (Id, Name) VALUES (1, 'John');";

        var createResult = await _connection.Query.ExecuteNonQueryAsync(createTableSql);
        Assert.That(createResult.IsSuccess, Is.True);

        var insertResult = await _connection.Query.ExecuteNonQueryAsync(insertSql);
        Assert.That(insertResult.IsSuccess, Is.True);
        Assert.That(insertResult.Data, Is.EqualTo(1));
    }

    [Test]
    public async Task ExecuteNonQueryAsync_WithInvalidQuery_ShouldReturnFailure()
    {
        var sql = "CREATE TALBE Fail (ID);";

        var result = await _connection.Query.ExecuteNonQueryAsync(sql);
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task ExecuteQueryAsync_WithValidQuery_ShouldReturnSuccess()
    {
        await _connection.Query.ExecuteNonQueryAsync(
            "CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL, Age SMALLINT);");
        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, 'John', 25);");
        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (2, 'Jane', 30);");

        var result = await _connection.Query.ExecuteQueryAsync("SELECT * FROM Users;");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data.TotalRows, Is.EqualTo(2));
        Assert.That(result.Data.Columns.Count, Is.EqualTo(3));
        Assert.That(result.Data.Columns[0].Name, Is.EqualTo("Id"));
        Assert.That(result.Data.Columns[0].Type, Is.EqualTo(CanonicalType.Int));
        Assert.That(result.Data.Columns[1].Name, Is.EqualTo("Name"));
        Assert.That(result.Data.Columns[1].Type, Is.EqualTo(CanonicalType.String));
        Assert.That(result.Data.Columns[2].Name, Is.EqualTo("Age"));
        Assert.That(result.Data.Columns[2].Type, Is.EqualTo(CanonicalType.SmallInt));
        Assert.That(result.Data.Rows[0][1], Is.EqualTo("John"));
        Assert.That(result.Data.Rows[1][1], Is.EqualTo("Jane"));
    }

    [Test]
    public async Task ExecuteQueryAsync_WithInvalidQuery_ShouldReturnFailure()
    {
        var result = await _connection.Query.ExecuteQueryAsync("SELECT * FROM NonExistentTable;");
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task ExecuteQueryAsync_WithEmptyTable_ShouldReturnNoRows()
    {
        await _connection.Query.ExecuteNonQueryAsync("CREATE TABLE Empty (Id INTEGER PRIMARY KEY);");

        var result = await _connection.Query.ExecuteQueryAsync("SELECT * FROM Empty;");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data.TotalRows, Is.EqualTo(0));
        Assert.That(result.Data.Columns.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task ExecuteQueryAsync_WithNullableColumn_ShouldReturnNull()
    {
        await _connection.Query.ExecuteNonQueryAsync("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT);");
        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, NULL);");

        var result = await _connection.Query.ExecuteQueryAsync("SELECT * FROM Users;");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data.Rows[0][1], Is.Null);
    }
    
    [Test]
    public async Task ExecuteScalarAsync_WithCountQuery_ShouldReturnCorrectCount()
    {
        await _connection.Query.ExecuteNonQueryAsync("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");
        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, 'John');");
        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (2, 'Jane');");

        var result = await _connection.Query.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Users;");
    
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.EqualTo(2));
    }

    [Test]
    public async Task ExecuteScalarAsync_WithInvalidQuery_ShouldReturnFailure()
    {
        var result = await _connection.Query.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM NonExistentTable;");
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task ExecuteScalarAsync_WithWrongType_ShouldReturnFailure()
    {
        await _connection.Query.ExecuteNonQueryAsync("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");
        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, 'John');");

        var result = await _connection.Query.ExecuteScalarAsync<int>("SELECT Name FROM Users;");
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task ExecuteScalarAsync_WithSingleValue_ShouldReturnCorrectValue()
    {
        await _connection.Query.ExecuteNonQueryAsync("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");
        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, 'John');");

        var result = await _connection.Query.ExecuteScalarAsync<string>("SELECT Name FROM Users WHERE Id = 1;");
    
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.EqualTo("John"));
    }
}