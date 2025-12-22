using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

/// <summary>
/// Represents an abstraction for a test installation of a virtual mod of the Petroglyph Star Wars game infrastructure.
/// </summary>
public interface ITestingVirtualModInstallation : ITestingModInstallation
{
    /// <summary>
    /// Gets the virtual mod associated of the test installation.
    /// </summary>
    new IVirtualMod Mod { get; }
}