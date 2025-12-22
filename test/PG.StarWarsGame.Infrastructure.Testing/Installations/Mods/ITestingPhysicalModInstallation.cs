using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

/// <summary>
/// Represents an abstraction for a test installation of a physical mod of the Petroglyph Star Wars game infrastructure.
/// </summary>
public interface ITestingPhysicalModInstallation : ITestingModInstallation, ITestingPhysicalPlayableObjectInstallation
{
    /// <summary>
    /// Gets the physical mod associated with this testing installation.
    /// </summary>
    /// <remarks>
    /// This property provides access to the <see cref="IPhysicalMod"/> instance that represents
    /// the mod being tested. The mod is expected to be located on the file system and adheres
    /// to the structure and requirements defined for physical mods.
    /// </remarks>
    new IPhysicalMod Mod { get; }

    /// <summary>
    /// Installs an invalid modinfo file to the mod installation.
    /// </summary>
    /// <param name="variantSubFileName">An optional name for the modinfo variant. If not specified, the main modinfo file is created.</param>
    /// <returns>An instance of <see cref="IModinfoFile"/> representing the installed, invalid modinfo file.</returns>
    IModinfoFile InstallInvalidModinfoFile(string? variantSubFileName = null);

    /// <summary>
    /// Installs a modinfo file to the mod installation.
    /// </summary>
    /// <param name="modinfo">The modinfo data to be installed.</param>
    /// <param name="variantSubFileName">An optional name for the modinfo variant. If not specified, the main modinfo file is created.</param>
    /// <returns>An instance of <see cref="IModinfoFile"/> representing the installed modinfo file.</returns>
    IModinfoFile InstallModinfoFile(IModinfo modinfo, string? variantSubFileName = null);
}