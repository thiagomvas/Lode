using Lode.Core;
using Lode.Tests.Common;

namespace Lode.Drivers.Sqlite.Tests;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Driver)]
[Category(TestCategories.Sqlite)]
public sealed class SqliteConnectionTests
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
    public async Task PingAsync_WithLiveConnection_ShouldReturnSuccess()
    {
        var result = await _driver.OpenConnectionAsync(_options);
        await using var connection = result.Data;
        
        var ping = await connection.PingAsync();

        Assert.That(ping.IsSuccess, Is.True);
    }
    
    
    [Test]
    public async Task PingAsync_WithDisposedConnection_ShouldReturnFailure()
    {
        var result = await _driver.OpenConnectionAsync(_options);
        await using var connection = result.Data;
        
        var successfulPing = await connection.PingAsync();
        Assert.That(successfulPing.IsSuccess, Is.True);

        await connection.DisposeAsync();
        var disposedPing = await connection.PingAsync();
        
        Assert.That(disposedPing.IsSuccess, Is.False);
    }
    
}
