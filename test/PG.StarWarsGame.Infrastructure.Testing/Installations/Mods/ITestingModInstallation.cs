using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

/// <summary>
/// Represents an abstraction for a test installation of a mod of the Petroglyph Star Wars game infrastructure.
/// </summary>
public interface ITestingModInstallation : ITestingModContainerInstallation
{
    /// <summary>
    /// Gets the mod associated with this testing mod installation.
    /// </summary>
    IMod Mod { get; }
}