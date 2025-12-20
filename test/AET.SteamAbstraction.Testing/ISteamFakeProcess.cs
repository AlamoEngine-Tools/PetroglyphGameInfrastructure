namespace AET.SteamAbstraction.Testing;

/// <summary>
/// Represents a fake Steam process for testing purposes.
/// </summary>
public interface ISteamFakeProcess
{
    /// <summary>
    /// Terminates the associated process.
    /// </summary>
    void Kill();
}