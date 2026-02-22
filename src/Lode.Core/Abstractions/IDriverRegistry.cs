namespace Lode.Core.Abstractions;

public interface IDriverRegistry
{
    void Register(IDbDriver driver);
    IDbDriver GetDriver(string driverName);
    IEnumerable<IDbDriver> GetDrivers();
    IEnumerable<string> GetDriverNames();
    bool TryGetDriver(string driverName, out IDbDriver driver);
}