using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Tests.Common;

namespace Lode.Drivers.Sqlite.Tests;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Driver)]
[Category(TestCategories.Sqlite)]
public class SqliteDriverTests
{
    [Test]
    public async Task OpenConnectionAsync_WithValidInMemoryDatabase_ReturnsSuccess()
    {
        var driver = new SqliteDriver();
        var opt = new DbConnectionOptions() { FilePath = ":memory:" };

        var result = await  driver.OpenConnectionAsync(opt);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
    }
}