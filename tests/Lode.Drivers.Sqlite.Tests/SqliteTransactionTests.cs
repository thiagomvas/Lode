using Lode.Core;
using Lode.Tests.Common;

namespace Lode.Drivers.Sqlite.Tests;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Driver)]
[Category(TestCategories.Sqlite)]
public sealed class SqliteTransactionTests
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
    public async Task BeginTransactionAsync_WithMultipleSuccessfulCommands_ShouldCommitAll()
    {
        await _connection.Query.ExecuteNonQueryAsync(
            "CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");

        var transactionResult = await _connection.BeginTransactionAsync();
        Assert.That(transactionResult.IsSuccess, Is.True);

        await using var transaction = transactionResult.Data;

        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, 'John');");
        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (2, 'Jane');");

        await transaction.CommitAsync();

        var result = await _connection.Query.ExecuteQueryAsync("SELECT * FROM Users;");
        Assert.That(result.Data.TotalRows, Is.EqualTo(2));
    }

    [Test]
    public async Task BeginTransactionAsync_WhenRolledBack_ShouldNotPersistChanges()
    {
        await _connection.Query.ExecuteNonQueryAsync(
            "CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");

        var transactionResult = await _connection.BeginTransactionAsync();
        await using var transaction = transactionResult.Data;

        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, 'John');");
        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (2, 'Jane');");

        await transaction.RollbackAsync();

        var result = await _connection.Query.ExecuteQueryAsync("SELECT * FROM Users;");
        Assert.That(result.Data.TotalRows, Is.EqualTo(0));
    }

    [Test]
    public async Task BeginTransactionAsync_WhenDisposedWithoutCommit_ShouldRollback()
    {
        await _connection.Query.ExecuteNonQueryAsync(
            "CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");

        var transactionResult = await _connection.BeginTransactionAsync();

        await using (var transaction = transactionResult.Data)
        {
            await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, 'John');");
        }

        var result = await _connection.Query.ExecuteQueryAsync("SELECT * FROM Users;");
        Assert.That(result.Data.TotalRows, Is.EqualTo(0));
    }

    [Test]
    public async Task BeginTransactionAsync_WhenOneCommandFails_ShouldRollbackAll()
    {
        await _connection.Query.ExecuteNonQueryAsync(
            "CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");

        var transactionResult = await _connection.BeginTransactionAsync();
        await using var transaction = transactionResult.Data;

        await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, 'John');");
        var failedInsert =
            await _connection.Query.ExecuteNonQueryAsync("INSERT INTO Users VALUES (1, 'Duplicate PK');");

        if (failedInsert.IsFailure)
            await transaction.RollbackAsync();

        var result = await _connection.Query.ExecuteQueryAsync("SELECT * FROM Users;");
        Assert.That(result.Data.TotalRows, Is.EqualTo(0));
    }
}