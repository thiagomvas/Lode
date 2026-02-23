using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using System.Diagnostics;

namespace Lode.Drivers.AccessDb;

public sealed class AccessDbConnection : IDbConnection
{
    private readonly string _file;
    private readonly IAccessDatabaseProvider _provider;

    public AccessDbConnection(string file)
    {
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("Access DB driver is only supported on Linux via MDB Tools.");

        _file = file;
        _provider = new LinuxMdbToolsProvider();

        Schema = new AccessDbSchemaProvider(file, _provider);
        Exporter = new AccessExporter(file, _provider);

    }

    public string FormattedName { get; init; }
    public ISchemaProvider Schema { get; }
    public IQueryExecutor Query => throw new NotSupportedException("Access Db Driver does not support querying.");
    public IImporter Importer => throw new NotSupportedException("Access Db Driver does not support write operations.");
    public IExporter Exporter { get; }

    public async Task<Result> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_file))
                return DriverErrors.ConnectionFailed("Access database file not found");

            var psi = new ProcessStartInfo
            {
                FileName = "mdb-tables",
                Arguments = $"-1 \"{_file}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);

            if (process == null)
                return DriverErrors.ConnectionFailed("Failed to start mdb-tables");

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
                return DriverErrors.ConnectionFailed("mdb-tools could not read the database");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return DriverErrors.ConnectionFailed(ex.Message);
        }
    }

    public Task<Result<IDbTransaction>> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            Result<IDbTransaction>.Failure(
                TransactionErrors.TransactionFailed("Transactions are not supported for Access MDBTools driver")
            )
        );
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}