using Lode.Core;
using Lode.Core.Abstractions;

namespace Lode.Cli;

public sealed class CliSession
{
    public IDbDriver Driver { get; set; }
    public IDbConnection Connection { get; set; }
    public DbConnectionOptions Options { get; set; }
    public IDriverRegistry Registry { get; set; }

    public bool IsConnected => Connection != null;
}