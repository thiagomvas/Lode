using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Core.Errors;
using Microsoft.Data.Sqlite;

namespace Lode.Drivers.Sqlite;

public sealed class SqliteTransaction : IDbTransaction
{
    private readonly Microsoft.Data.Sqlite.SqliteTransaction _transaction;
    private bool _completed;

    public SqliteTransaction(Microsoft.Data.Sqlite.SqliteTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task<Result> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _transaction.CommitAsync(cancellationToken);
            _completed = true;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return TransactionErrors.TransactionFailed(ex.Message);
        }
    }

    public async Task<Result> RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _transaction.RollbackAsync(cancellationToken);
            _completed = true;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return TransactionErrors.TransactionFailed(ex.Message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_completed)
            await _transaction.RollbackAsync();

        await _transaction.DisposeAsync();
    }
}