using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Drivers.Sqlite;

var driver = new SqliteDriver();

var opt = new DbConnectionOptions()
{
    FilePath = "/home/thiagomv/database.db"
};

var connResult = await driver.OpenConnectionAsync(opt);

if (connResult.IsFailure)
{
    Console.WriteLine($"Failed to connect: {string.Join(", ", connResult.Errors.Select(e => e.Message))}");
    return;
}

await using var connection = connResult.Data;

var ping = await connection.PingAsync();
Console.WriteLine(ping.IsSuccess ? "Ping successful" : "Ping failed");
Console.WriteLine("Enter SQL (or 'exit' to quit, ';' to execute multiline, 'begin'/'commit'/'rollback' for transactions):");

IDbTransaction? activeTransaction = null;
var buffer = new System.Text.StringBuilder();

while (true)
{
    Console.Write(activeTransaction is not null ? "transaction> " : buffer.Length > 0 ? "        ... " : "> ");
    var line = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(line)) continue;
    if (line.Equals("tables", StringComparison.OrdinalIgnoreCase))
    {
        var result = await connection.Schema.GetTableNamesAsync();
        if (result.IsFailure)
        {
            Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            continue;
        }
        foreach (var table in result.Data)
            Console.WriteLine(table);
        continue;
    }

    if (line.StartsWith("table ", StringComparison.OrdinalIgnoreCase))
    {
        var tableName = line[6..].Trim();
        var result = await connection.Schema.GetTableDefinitionAsync(tableName);
        if (result.IsFailure)
        {
            Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            continue;
        }
        Console.WriteLine($"Table: {result.Data.Name}");
        Console.WriteLine(new string('-', 40));
        foreach (var col in result.Data.Columns)
            Console.WriteLine($"  {col.Name} ({col.Type})");
        continue;
    }

    if (line.Equals("schema", StringComparison.OrdinalIgnoreCase))
    {
        var result = await connection.Schema.GetSchemaAsync();
        if (result.IsFailure)
        {
            Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            continue;
        }
        Console.WriteLine(result.Data);
        continue;
    }
    if (line.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        if (activeTransaction is not null)
        {
            await activeTransaction.RollbackAsync();
            await activeTransaction.DisposeAsync();
            Console.WriteLine("Active transaction rolled back.");
        }
        break;
    }

    if (line.Equals("begin", StringComparison.OrdinalIgnoreCase))
    {
        if (activeTransaction is not null)
        {
            Console.WriteLine("Error: transaction already active.");
            continue;
        }
        var txResult = await connection.BeginTransactionAsync();
        if (txResult.IsFailure)
            Console.WriteLine($"Error: {string.Join(", ", txResult.Errors.Select(e => e.Message))}");
        else
        {
            activeTransaction = txResult.Data;
            Console.WriteLine("Transaction started.");
        }
        continue;
    }

    if (line.Equals("commit", StringComparison.OrdinalIgnoreCase))
    {
        if (activeTransaction is null) { Console.WriteLine("Error: no active transaction."); continue; }
        var result = await activeTransaction.CommitAsync();
        Console.WriteLine(result.IsSuccess ? "Transaction committed." : $"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        await activeTransaction.DisposeAsync();
        activeTransaction = null;
        continue;
    }

    if (line.Equals("rollback", StringComparison.OrdinalIgnoreCase))
    {
        if (activeTransaction is null) { Console.WriteLine("Error: no active transaction."); continue; }
        var result = await activeTransaction.RollbackAsync();
        Console.WriteLine(result.IsSuccess ? "Transaction rolled back." : $"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        await activeTransaction.DisposeAsync();
        activeTransaction = null;
        continue;
    }

    buffer.AppendLine(line);

    if (!line.EndsWith(';')) continue;

    var sql = buffer.ToString().Trim();
    buffer.Clear();

    await ExecuteSqlAsync(sql);
}

async Task ExecuteSqlAsync(string sql)
{
    var upper = sql.ToUpperInvariant().TrimStart();

    if (upper.StartsWith("SELECT"))
    {
        var result = await connection.Query.ExecuteQueryAsync(sql);
        if (result.IsFailure)
        {
            Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            return;
        }

        var columns = string.Join(" | ", result.Data.Columns.Select(c => c.Name));
        Console.WriteLine(columns);
        Console.WriteLine(new string('-', columns.Length));

        foreach (var row in result.Data.Rows)
            Console.WriteLine(string.Join(" | ", row.Select(v => v?.ToString() ?? "NULL")));

        Console.WriteLine($"\n{result.Data.TotalRows} row(s) returned");
    }
    
    else
    {
        var result = await connection.Query.ExecuteNonQueryAsync(sql);
        if (result.IsFailure)
            Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        else
            Console.WriteLine($"{result.Data} row(s) affected");
    }
}