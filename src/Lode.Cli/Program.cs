using Lode.Business;
using Lode.Core;
using Lode.Core.Abstractions;
using Lode.Drivers.Sqlite;

var driver = new SqliteDriver();

var opt = new DbConnectionOptions()
{
    FilePath = "/home/thiagomv/database.db"
};

var opt2 = new DbConnectionOptions()
{
    FilePath = "/home/thiagomv/database3.db"
};

var connResult = await driver.OpenConnectionAsync(opt);

if (connResult.IsFailure)
{
    Console.WriteLine($"Failed to connect: {string.Join(", ", connResult.Errors.Select(e => e.Message))}");
    return;
}

await using var source = connResult.Data;

var ping = await source.PingAsync();
Console.WriteLine(ping.IsSuccess ? "Ping successful" : "Ping failed");

var destResult = await driver.OpenConnectionAsync(opt2);

if (destResult.IsFailure)
{
    Console.WriteLine($"Failed to connect: {string.Join(", ", destResult.Errors.Select(e => e.Message))}");
    return;
}

await using var dest = destResult.Data;

await dest.Query.ExecuteNonQueryAsync("""
                                      PRAGMA foreign_keys = OFF;

                                      BEGIN TRANSACTION;

                                      SELECT 'DROP TABLE IF EXISTS "' || name || '";'
                                      FROM sqlite_master
                                      WHERE type='table' AND name NOT LIKE 'sqlite_%';

                                      COMMIT;

                                      PRAGMA foreign_keys = ON;
                                      """);

IMigrationService migrationService = new MigrationService();

Console.WriteLine("Migrating database");

var result = await migrationService.MigrateAsync(source, dest);

if (result.IsFailure)
{
    Console.WriteLine($"Migration failed: {string.Join(", ", result.Errors.Select(e => e.Message))}");
    return;
}

Console.WriteLine("Migration completed successfully");