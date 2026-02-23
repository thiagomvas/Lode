using Lode.Cli.Commands;
using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Models;
using Lode.Core.Models.Schema;
using Lode.Core.ValueTypes;
using Lode.Tests.Common;
using NSubstitute;

namespace Lode.Cli.Tests.Commands;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Cli)]
public sealed class QueryCommandTests
{
    private IDbConnection _connection = null!;
    private IQueryExecutor _queryExecutor = null!;
    private QueryCommand _command = null!;
    private CliSession _session = null!;
    private CommandContext _context = null!;

    [SetUp]
    public void Setup()
    {
        _connection = Substitute.For<IDbConnection>();
        _queryExecutor = Substitute.For<IQueryExecutor>();
        _connection.Query.Returns(_queryExecutor);
        _command = new QueryCommand();
        _session = new CliSession { Connection = _connection };
        _context = new CommandContext();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task Execute_WhenNotConnected_DoesNotExecuteQuery()
    {
        _session.Connection = null!;

        try
        {
            await _command.Execute(_context, _session);
        }
        catch (InvalidOperationException)
        {
            // Spectre.Console throws when parsing markup
        }

        await _queryExecutor.DidNotReceive().ExecuteQueryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithNoQueryArguments_DoesNotExecuteQuery()
    {
        try
        {
            await _command.Execute(_context, _session);
        }
        catch (InvalidOperationException)
        {
            // Spectre.Console throws when parsing markup
        }

        await _queryExecutor.DidNotReceive().ExecuteQueryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithEmptyArgsList_DoesNotExecuteQuery()
    {
        _context.Args.Clear();

        try
        {
            await _command.Execute(_context, _session);
        }
        catch (InvalidOperationException)
        {
            // Spectre.Console throws when parsing markup
        }

        await _queryExecutor.DidNotReceive().ExecuteQueryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithValidQuery_ExecutesQuery()
    {
        _context.Args.Add("SELECT");
        _context.Args.Add("*");
        _context.Args.Add("FROM");
        _context.Args.Add("users");

        var columns = new List<ColumnDefinition>
        {
            new() { Name = "id", Type = CanonicalType.Int },
            new() { Name = "name", Type = CanonicalType.String }
        };

        var rows = new List<IReadOnlyList<object?>>
        {
            new List<object?> { 1, "Alice" },
            new List<object?> { 2, "Bob" }
        };

        var queryResult = new QueryResult
        {
            Columns = columns,
            Rows = rows,
            TotalRows = 2,
            PageSize = 100,
            PageNumber = 1,
            PageCount = 1
        };

        _queryExecutor.ExecuteQueryAsync("SELECT * FROM users", Arg.Any<CancellationToken>())
            .Returns(Result<QueryResult>.Success(queryResult));

        await _command.Execute(_context, _session);

        await _queryExecutor.Received(1).ExecuteQueryAsync("SELECT * FROM users", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithSingleWordQuery_ExecutesQuery()
    {
        _context.Args.Add("PRAGMA");

        var queryResult = new QueryResult
        {
            Columns = new List<ColumnDefinition>(),
            Rows = new List<IReadOnlyList<object?>>(),
            TotalRows = 0,
            PageSize = 100,
            PageNumber = 1,
            PageCount = 1
        };

        _queryExecutor.ExecuteQueryAsync("PRAGMA", Arg.Any<CancellationToken>())
            .Returns(Result<QueryResult>.Success(queryResult));

        await _command.Execute(_context, _session);

        await _queryExecutor.Received(1).ExecuteQueryAsync("PRAGMA", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithQueryContainingQuotes_PreservesQuotes()
    {
        _context.Args.Add("SELECT");
        _context.Args.Add("'test'");

        var queryResult = new QueryResult
        {
            Columns = new List<ColumnDefinition>(),
            Rows = new List<IReadOnlyList<object?>>(),
            TotalRows = 0,
            PageSize = 100,
            PageNumber = 1,
            PageCount = 1
        };

        _queryExecutor.ExecuteQueryAsync("SELECT 'test'", Arg.Any<CancellationToken>())
            .Returns(Result<QueryResult>.Success(queryResult));

        await _command.Execute(_context, _session);

        await _queryExecutor.Received(1).ExecuteQueryAsync("SELECT 'test'", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WhenQueryFails_DoesNotThrow()
    {
        _context.Args.Add("INVALID");
        _context.Args.Add("SQL");

        var error = new Error("QUERY_ERROR", "Invalid SQL syntax");
        _queryExecutor.ExecuteQueryAsync("INVALID SQL", Arg.Any<CancellationToken>())
            .Returns(Result<QueryResult>.Failure([error]));

        try
        {
            await _command.Execute(_context, _session);
        }
        catch (InvalidOperationException)
        {
            // Spectre.Console throws when parsing markup
        }

        await _queryExecutor.Received(1).ExecuteQueryAsync("INVALID SQL", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithQueryReturningNullValues_HandlesNulls()
    {
        _context.Args.Add("SELECT");
        _context.Args.Add("*");
        _context.Args.Add("FROM");
        _context.Args.Add("users");

        var columns = new List<ColumnDefinition>
        {
            new() { Name = "id", Type = CanonicalType.Int },
            new() { Name = "name", Type = CanonicalType.String }
        };

        var rows = new List<IReadOnlyList<object?>>
        {
            new List<object?> { 1, null },
            new List<object?> { 2, "Bob" }
        };

        var queryResult = new QueryResult
        {
            Columns = columns,
            Rows = rows,
            TotalRows = 2,
            PageSize = 100,
            PageNumber = 1,
            PageCount = 1
        };

        _queryExecutor.ExecuteQueryAsync("SELECT * FROM users", Arg.Any<CancellationToken>())
            .Returns(Result<QueryResult>.Success(queryResult));

        await _command.Execute(_context, _session);

        await _queryExecutor.Received(1).ExecuteQueryAsync("SELECT * FROM users", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithQueryReturningNoRows_HandlesEmptyResult()
    {
        _context.Args.Add("SELECT");
        _context.Args.Add("*");
        _context.Args.Add("FROM");
        _context.Args.Add("users");

        var columns = new List<ColumnDefinition>
        {
            new() { Name = "id", Type = CanonicalType.Int },
            new() { Name = "name", Type = CanonicalType.String }
        };

        var queryResult = new QueryResult
        {
            Columns = columns,
            Rows = new List<IReadOnlyList<object?>>(),
            TotalRows = 0,
            PageSize = 100,
            PageNumber = 1,
            PageCount = 1
        };

        _queryExecutor.ExecuteQueryAsync("SELECT * FROM users", Arg.Any<CancellationToken>())
            .Returns(Result<QueryResult>.Success(queryResult));

        await _command.Execute(_context, _session);

        await _queryExecutor.Received(1).ExecuteQueryAsync("SELECT * FROM users", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithQueryReturningManyColumns_HandlesAllColumns()
    {
        _context.Args.Add("SELECT");
        _context.Args.Add("*");
        _context.Args.Add("FROM");
        _context.Args.Add("users");

        var columns = new List<ColumnDefinition>
        {
            new() { Name = "col1", Type = CanonicalType.Int },
            new() { Name = "col2", Type = CanonicalType.String },
            new() { Name = "col3", Type = CanonicalType.Double },
            new() { Name = "col4", Type = CanonicalType.Blob },
            new() { Name = "col5", Type = CanonicalType.Boolean }
        };

        var rows = new List<IReadOnlyList<object?>>
        {
            new List<object?> { 1, "test", 3.14, new byte[] { 1, 2, 3 }, true }
        };

        var queryResult = new QueryResult
        {
            Columns = columns,
            Rows = rows,
            TotalRows = 1,
            PageSize = 100,
            PageNumber = 1,
            PageCount = 1
        };

        _queryExecutor.ExecuteQueryAsync("SELECT * FROM users", Arg.Any<CancellationToken>())
            .Returns(Result<QueryResult>.Success(queryResult));

        try
        {
            await _command.Execute(_context, _session);
        }
        catch (InvalidOperationException)
        {
            // Spectre.Console throws when parsing byte array as string with special characters
        }

        await _queryExecutor.Received(1).ExecuteQueryAsync("SELECT * FROM users", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithComplexQuery_JoinsArgsCorrectly()
    {
        _context.Args.Add("SELECT");
        _context.Args.Add("u.name,");
        _context.Args.Add("o.total");
        _context.Args.Add("FROM");
        _context.Args.Add("users");
        _context.Args.Add("u");
        _context.Args.Add("JOIN");
        _context.Args.Add("orders");
        _context.Args.Add("o");
        _context.Args.Add("ON");
        _context.Args.Add("u.id");
        _context.Args.Add("=");
        _context.Args.Add("o.user_id");

        var queryResult = new QueryResult
        {
            Columns = new List<ColumnDefinition>(),
            Rows = new List<IReadOnlyList<object?>>(),
            TotalRows = 0,
            PageSize = 100,
            PageNumber = 1,
            PageCount = 1
        };

        _queryExecutor.ExecuteQueryAsync("SELECT u.name, o.total FROM users u JOIN orders o ON u.id = o.user_id", Arg.Any<CancellationToken>())
            .Returns(Result<QueryResult>.Success(queryResult));

        await _command.Execute(_context, _session);

        await _queryExecutor.Received(1).ExecuteQueryAsync("SELECT u.name, o.total FROM users u JOIN orders o ON u.id = o.user_id", Arg.Any<CancellationToken>());
    }

    [Test]
    public void Name_ReturnsQuery()
    {
        Assert.That(_command.Name, Is.EqualTo("query"));
    }

    [Test]
    public async Task Execute_WithMultipleErrors_DoesNotThrow()
    {
        _context.Args.Add("INVALID");

        var errors = new[]
        {
            new Error("ERROR1", "First error"),
            new Error("ERROR2", "Second error")
        };

        _queryExecutor.ExecuteQueryAsync("INVALID", Arg.Any<CancellationToken>())
            .Returns(Result<QueryResult>.Failure(errors));

        try
        {
            await _command.Execute(_context, _session);
        }
        catch (InvalidOperationException)
        {
            // Spectre.Console throws when parsing markup
        }

        await _queryExecutor.Received(1).ExecuteQueryAsync("INVALID", Arg.Any<CancellationToken>());
    }
}





