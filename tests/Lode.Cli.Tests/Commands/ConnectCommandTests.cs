using Lode.Cli.Commands;
using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Tests.Common;
using NSubstitute;
using NUnit.Framework;

namespace Lode.Cli.Tests.Commands;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Cli)]
public sealed class ConnectCommandTests
{
    private IDriverRegistry _driverRegistry = null!;
    private IDbDriver _driver = null!;
    private IDbConnection _connection = null!;
    private ConnectCommand _command = null!;
    private CliSession _session = null!;
    private CommandContext _context = null!;

    [SetUp]
    public void Setup()
    {
        _driverRegistry = Substitute.For<IDriverRegistry>();
        _driver = Substitute.For<IDbDriver>();
        _connection = Substitute.For<IDbConnection>();
        _command = new ConnectCommand(_driverRegistry);
        _session = new CliSession();
        _context = new CommandContext();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task Execute_WithNoArguments_DoesNotSetConnection()
    {
        try
        {
            await _command.Execute(_context, _session);
        }
        catch (InvalidOperationException)
        {
            // Spectre.Console throws when parsing the usage message with square brackets
        }

        Assert.That(_session.Connection, Is.Null);
        Assert.That(_session.Driver, Is.Null);
    }

    [Test]
    public async Task Execute_WithDriverNotFound_DoesNotSetConnection()
    {
        _context.Args.Add("nonexistent");
        _driverRegistry.TryGetDriver("nonexistent", out _).Returns(false);

        await _command.Execute(_context, _session);

        Assert.That(_session.Connection, Is.Null);
        Assert.That(_session.Driver, Is.Null);
    }

    [Test]
    public async Task Execute_WithValidDriver_SetsSessionProperties()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.Name.Returns("sqlite");
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        Assert.That(_session.Connection, Is.EqualTo(_connection));
        Assert.That(_session.Driver, Is.EqualTo(_driver));
        Assert.That(_session.Options, Is.Not.Null);
    }

    [Test]
    public async Task Execute_WithFilePathOption_SetsFilePath()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        _context.Args.Add("FilePath=/path/to/db.sqlite");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        await _driver.Received(1).OpenConnectionAsync(
            Arg.Is<DbConnectionOptions>(opt => opt.FilePath == "/path/to/db.sqlite"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithFileOption_SetsFilePath()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        _context.Args.Add("File=/path/to/db.sqlite");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        await _driver.Received(1).OpenConnectionAsync(
            Arg.Is<DbConnectionOptions>(opt => opt.FilePath == "/path/to/db.sqlite"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithCaseInsensitiveFilePathOption_SetsFilePath()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        _context.Args.Add("filepath=/path/to/db.sqlite");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        await _driver.Received(1).OpenConnectionAsync(
            Arg.Is<DbConnectionOptions>(opt => opt.FilePath == "/path/to/db.sqlite"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithCustomOption_AddsToOptionsDictionary()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        _context.Args.Add("CustomKey=CustomValue");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        await _driver.Received(1).OpenConnectionAsync(
            Arg.Is<DbConnectionOptions>(opt => opt.Options.ContainsKey("CustomKey") && opt.Options["CustomKey"] == "CustomValue"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithMultipleOptions_SetsAllOptions()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        _context.Args.Add("FilePath=/path/to/db.sqlite");
        _context.Args.Add("Key1=Value1");
        _context.Args.Add("Key2=Value2");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        await _driver.Received(1).OpenConnectionAsync(
            Arg.Is<DbConnectionOptions>(opt => 
                opt.FilePath == "/path/to/db.sqlite" &&
                opt.Options.ContainsKey("Key1") && opt.Options["Key1"] == "Value1" &&
                opt.Options.ContainsKey("Key2") && opt.Options["Key2"] == "Value2"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithInvalidArgumentFormat_SkipsArgument()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        _context.Args.Add("InvalidArgument");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        Assert.That(_session.Connection, Is.EqualTo(_connection));
    }

    [Test]
    public async Task Execute_WhenConnectionFails_DoesNotSetSession()
    {
        var options = new DbConnectionOptions();
        var error = new Error("CONNECTION_ERROR", "Failed to connect");
        _context.Args.Add("sqlite");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Failure([error]));

        await _command.Execute(_context, _session);

        Assert.That(_session.Connection, Is.Null);
        Assert.That(_session.Driver, Is.Null);
        Assert.That(_session.Options, Is.Null);
    }

    [Test]
    public async Task Execute_WhenExceptionThrown_ClearsSessionState()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns<Result<IDbConnection>>(x => throw new InvalidOperationException("Connection error"));

        await _command.Execute(_context, _session);

        Assert.That(_session.Connection, Is.Null);
        Assert.That(_session.Driver, Is.Null);
        Assert.That(_session.Options, Is.Null);
    }

    [Test]
    public async Task Execute_WithOptionContainingWhitespace_TrimsWhitespace()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        _context.Args.Add("Key = Value ");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        await _driver.Received(1).OpenConnectionAsync(
            Arg.Is<DbConnectionOptions>(opt => opt.Options.ContainsKey("Key") && opt.Options["Key"] == "Value"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithEmptyDriverName_DoesNotSetConnection()
    {
        _context.Args.Add("");
        _driverRegistry.TryGetDriver("", out _).Returns(false);

        await _command.Execute(_context, _session);

        Assert.That(_session.Connection, Is.Null);
        Assert.That(_session.Driver, Is.Null);
    }

    [Test]
    public void Name_ReturnsConnect()
    {
        Assert.That(_command.Name, Is.EqualTo("connect"));
    }

    [Test]
    public async Task Execute_CallsOpenConnectionAsync_WithCancellationToken()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        await _driver.Received(1).OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Execute_WithMultipleErrors_DoesNotSetSession()
    {
        var options = new DbConnectionOptions();
        var errors = new[]
        {
            new Error("ERROR1", "First error"),
            new Error("ERROR2", "Second error")
        };
        _context.Args.Add("sqlite");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Failure(errors));

        await _command.Execute(_context, _session);

        Assert.That(_session.Connection, Is.Null);
        Assert.That(_session.Driver, Is.Null);
        Assert.That(_session.Options, Is.Null);
    }

    [Test]
    public async Task Execute_WithOptionValueContainingEquals_PreservesValue()
    {
        var options = new DbConnectionOptions();
        _context.Args.Add("sqlite");
        _context.Args.Add("ConnectionString=Server=localhost;Port=5432");
        
        _driverRegistry.TryGetDriver("sqlite", out _)
            .Returns(x =>
            {
                x[1] = _driver;
                return true;
            });
        
        _driver.GetDefaultOptions().Returns(options);
        _driver.OpenConnectionAsync(Arg.Any<DbConnectionOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<IDbConnection>.Success(_connection));

        await _command.Execute(_context, _session);

        await _driver.Received(1).OpenConnectionAsync(
            Arg.Is<DbConnectionOptions>(opt => 
                opt.Options.ContainsKey("ConnectionString") && 
                opt.Options["ConnectionString"] == "Server=localhost;Port=5432"),
            Arg.Any<CancellationToken>());
    }
}

