using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using Lode.Core.Models;
using Lode.Core.Models.Schema;
using Lode.Tests.Common;
using NSubstitute;

namespace Lode.Business.Tests;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Business)]
public class MigrationServiceTests
{
    private IDbConnection _source;
    private IDbConnection _destination;
    private ISchemaProvider _sourceSchema;
    private IExporter _exporter;
    private IImporter _importer;
    private MigrationService _sut;

    [SetUp]
    public void Setup()
    {
        _sourceSchema = Substitute.For<ISchemaProvider>();
        _exporter = Substitute.For<IExporter>();
        _importer = Substitute.For<IImporter>();

        _source = Substitute.For<IDbConnection>();
        _source.Schema.Returns(_sourceSchema);
        _source.Exporter.Returns(_exporter);

        _destination = Substitute.For<IDbConnection>();
        _destination.Importer.Returns(_importer);

        _sut = new MigrationService();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _source.DisposeAsync();
        await _destination.DisposeAsync();
    }

    [Test]
    public async Task MigrateAsync_WhenGetTableNamesFails_ShouldReturnFailure()
    {
        _sourceSchema.GetTableNamesAsync()
            .Returns(Result<IEnumerable<string>>.Failure(new Error("schema", "Could not read tables")));

        var result = await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task MigrateAsync_WhenGetTableNamesFails_ShouldPropagateErrors()
    {
        var error = new Error("schema", "Could not read tables");
        _sourceSchema.GetTableNamesAsync()
            .Returns(Result<IEnumerable<string>>.Failure(error));

        var result = await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        Assert.That(result.Errors, Contains.Item(error));
    }

    [Test]
    public async Task MigrateAsync_WhenGetTableNamesFails_ShouldNotCallImporter()
    {
        _sourceSchema.GetTableNamesAsync()
            .Returns(Result<IEnumerable<string>>.Failure(new Error("schema", "Could not read tables")));

        await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        await _importer.DidNotReceiveWithAnyArgs().ImportAsync(default!, default!, default);
    }

    [Test]
    public async Task MigrateAsync_WithEmptyTablesArgument_ShouldMigrateAllSourceTables()
    {
        SetupSourceTables(new[] { "Users", "Products" });
        SetupTableDefinition("Users", new TableDefinition());
        SetupTableDefinition("Products", new TableDefinition());

        var result = await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        Assert.That(result.IsSuccess, Is.True);
        await _sourceSchema.Received(1).GetTableDefinitionAsync("Users");
        await _sourceSchema.Received(1).GetTableDefinitionAsync("Products");
    }

    [Test]
    public async Task MigrateAsync_WithNoSourceTables_ShouldReturnSuccess()
    {
        SetupSourceTables(Array.Empty<string>());

        var result = await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        Assert.That(result.IsSuccess, Is.True);
        await _importer.DidNotReceiveWithAnyArgs().ImportAsync(default!, default!, default);
    }

    [Test]
    public async Task MigrateAsync_WithSpecificTables_ShouldOnlyMigrateRequestedTables()
    {
        SetupSourceTables(new[] { "Users", "Products", "Orders" });
        SetupTableDefinition("Users", new TableDefinition());

        var result = await _sut.MigrateAsync(_source, _destination, tables: new[] { "Users" });

        Assert.That(result.IsSuccess, Is.True);
        await _sourceSchema.Received(1).GetTableDefinitionAsync("Users");
        await _sourceSchema.DidNotReceive().GetTableDefinitionAsync("Products");
        await _sourceSchema.DidNotReceive().GetTableDefinitionAsync("Orders");
    }

    [Test]
    public async Task MigrateAsync_WithNonExistentTable_ShouldReturnFailure()
    {
        SetupSourceTables(new[] { "Users" });

        var result = await _sut.MigrateAsync(_source, _destination, tables: new[] { "NonExistent" });

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task MigrateAsync_WithNonExistentTable_ShouldNotCallImporter()
    {
        SetupSourceTables(new[] { "Users" });

        await _sut.MigrateAsync(_source, _destination, tables: new[] { "NonExistent" });

        await _importer.DidNotReceiveWithAnyArgs().ImportAsync(default!, default!, default);
    }

    [Test]
    public async Task MigrateAsync_WithTableNameDifferingOnlyInCase_ShouldSucceed()
    {
        SetupSourceTables(new[] { "Users" });
        SetupTableDefinition("users", new TableDefinition());

        var result = await _sut.MigrateAsync(_source, _destination, tables: new[] { "users" });

        Assert.That(result.IsSuccess, Is.True);
    }
    
    [Test]
    public async Task MigrateAsync_WhenGetTableDefinitionFails_ShouldReturnFailure()
    {
        SetupSourceTables(new[] { "Users" });
        _sourceSchema.GetTableDefinitionAsync("Users")
            .Returns(Result<TableDefinition>.Failure(new Error("schema", "Could not read definition")));

        var result = await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task MigrateAsync_WhenGetTableDefinitionFails_ShouldPropagateErrors()
    {
        var error = new Error("schema", "Could not read definition");
        SetupSourceTables(new[] { "Users" });
        _sourceSchema.GetTableDefinitionAsync("Users")
            .Returns(Result<TableDefinition>.Failure(error));

        var result = await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        Assert.That(result.Errors, Contains.Item(error));
    }

    [Test]
    public async Task MigrateAsync_WhenFirstTableDefinitionFails_ShouldNotProcessRemainingTables()
    {
        SetupSourceTables(new[] { "Users", "Products" });
        _sourceSchema.GetTableDefinitionAsync("Users")
            .Returns(Result<TableDefinition>.Failure(new Error("schema", "Failed")));

        await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        await _sourceSchema.DidNotReceive().GetTableDefinitionAsync("Products");
    }
    
    [Test]
    public async Task MigrateAsync_ShouldExportEachTable()
    {
        SetupSourceTables(new[] { "Users", "Products" });
        SetupTableDefinition("Users", new TableDefinition());
        SetupTableDefinition("Products", new TableDefinition());

        await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        _exporter.Received(1).ExportAsync("Users", Arg.Any<CancellationToken>());
        _exporter.Received(1).ExportAsync("Products", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task MigrateAsync_ShouldPassExportedRowsToImporter()
    {
        var definition = new TableDefinition();
        var rows = AsyncEnumerable.Empty<CanonicalRow>();

        SetupSourceTables(new[] { "Users" });
        SetupTableDefinition("Users", definition);
        _exporter.ExportAsync("Users", Arg.Any<CancellationToken>()).Returns(rows);

        await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        await _importer.Received(1).ImportAsync(rows, definition, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task MigrateAsync_ShouldPassCancellationTokenToExporter()
    {
        var cts = new CancellationTokenSource();
        SetupSourceTables(new[] { "Users" });
        SetupTableDefinition("Users", new TableDefinition());

        await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>(), cancellationToken: cts.Token);

        _exporter.Received(1).ExportAsync("Users", cts.Token);
    }

    [Test]
    public async Task MigrateAsync_ShouldPassCancellationTokenToImporter()
    {
        var cts = new CancellationTokenSource();
        SetupSourceTables(new[] { "Users" });
        SetupTableDefinition("Users", new TableDefinition());

        await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>(), cancellationToken: cts.Token);

        await _importer.Received(1).ImportAsync(
            Arg.Any<IAsyncEnumerable<CanonicalRow>>(),
            Arg.Any<TableDefinition>(),
            cts.Token);
    }

    [Test]
    public async Task MigrateAsync_WhenEverythingSucceeds_ShouldReturnSuccess()
    {
        SetupSourceTables(new[] { "Users", "Products" });
        SetupTableDefinition("Users", new TableDefinition());
        SetupTableDefinition("Products", new TableDefinition());

        var result = await _sut.MigrateAsync(_source, _destination, tables: Array.Empty<string>());

        Assert.That(result.IsSuccess, Is.True);
    }

    private void SetupSourceTables(IEnumerable<string> tableNames)
    {
        _sourceSchema.GetTableNamesAsync()
            .Returns(Result<IEnumerable<string>>.Success(tableNames));
    }

    private void SetupTableDefinition(string tableName, TableDefinition definition)
    {
        _sourceSchema.GetTableDefinitionAsync(tableName)
            .Returns(Result<TableDefinition>.Success(definition));
    }
}