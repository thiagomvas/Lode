using Lode.Core.Abstractions;

namespace Lode.Business;

public sealed class DriverRegistry : IDriverRegistry
{
    private readonly Dictionary<string, IDbDriver> _drivers =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(IDbDriver driver)
    {
        if (driver == null)
            throw new ArgumentNullException(nameof(driver));

        if (string.IsNullOrWhiteSpace(driver.Name))
            throw new ArgumentException("Driver must have a valid name", nameof(driver));

        _drivers[driver.Name] = driver;
    }

    public IDbDriver GetDriver(string driverName)
    {
        if (string.IsNullOrWhiteSpace(driverName))
            throw new ArgumentException("Driver name cannot be null or empty", nameof(driverName));

        if (!_drivers.TryGetValue(driverName, out var driver))
            throw new InvalidOperationException($"Driver '{driverName}' is not registered");

        return driver;
    }

    public IEnumerable<IDbDriver> GetDrivers()
    {
        return _drivers.Values;
    }

    public IEnumerable<string> GetDriverNames()
    {
        return _drivers.Keys;
    }

    public bool TryGetDriver(string driverName, out IDbDriver driver)
    {
        if (string.IsNullOrWhiteSpace(driverName))
        {
            driver = null;
            return false;
        }

        return _drivers.TryGetValue(driverName, out driver);
    }
}