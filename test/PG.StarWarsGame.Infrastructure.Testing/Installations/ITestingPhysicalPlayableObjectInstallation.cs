using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

/// <summary>
/// Represents an abstraction for a test installation that provides access
/// to a <see cref="IPhysicalPlayableObject"/> and its associated <see cref="ITestingGameInstallation"/>.
/// </summary>
public interface ITestingPhysicalPlayableObjectInstallation : ITestingPlayableObjectInstallation
{
    /// <summary>
    /// Gets the physical playable object associated with this test installation.
    /// This object represents a playable entity stored on the file system, such as a game or mod,
    /// and provides access to its directory and other related properties.
    /// </summary>
    new IPhysicalPlayableObject PlayableObject { get; }

    /// <summary>
    /// Installs the specified language for the playable object.
    /// </summary>
    /// <param name="language">The language information to be installed.</param>
    void InstallLanguage(ILanguageInfo language);
}