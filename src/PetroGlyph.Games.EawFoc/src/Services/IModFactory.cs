using System.Globalization;
using System.IO;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services;

/// <summary>
/// Factory to create mods.
/// </summary>
public interface IModFactory
{
    /// <summary>
    /// Creates a new <see cref="IPhysicalMod"/> instance for the specified game from the specified mod reference.
    /// </summary>
    /// <remarks>
    /// The created mod is not added to the <see cref="IModContainer.Mods"/> collection of <paramref name="game"/>.
    /// </remarks>
    /// <param name="game">The <see cref="IGame"/> of the mod.</param>
    /// <param name="modReference">The mod reference to use for creating the mod instance.</param>
    /// <param name="culture">The culture that shall be used to determine the mod's name.</param>
    /// <returns>Teh created mod instance.</returns>
    /// <exception cref="ModNotFoundException"> The directory information of <paramref name="modReference"/> does not exist.</exception>
    /// <exception cref="ModException">
    /// <paramref name="modReference"/> or identified modinfo files had illegal information, such as empty names.
    /// </exception>
    IPhysicalMod CreatePhysicalMod(IGame game, DetectedModReference modReference, CultureInfo culture);

    /// <summary>
    /// Creates a virtual mods from a given <see cref="IModinfo"/>.
    /// </summary>
    /// <remarks>The created mods do NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
    /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
    /// <param name="virtualModInfo">Set of <see cref="IModinfo"/> where each element defines its own virtual mod.</param>
    /// <returns>One or many virtual mods</returns>
    /// <exception cref="PetroglyphException">if the virtual mod could not be created.</exception>
    IVirtualMod CreateVirtualMod(IGame game, IModinfo virtualModInfo);

    /// <summary>
    /// Creates virtual mods for a game.
    /// </summary>
    /// <remarks>The created mods dot NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
    /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
    /// <param name="name">The name of the virtual mod.</param>
    /// <param name="dependencies">The dependencies of the virtual mod.</param>
    /// <returns>One or many virtual mods</returns>
    /// <exception cref="PetroglyphException">if the virtual mod could not be created.</exception>
    IVirtualMod CreateVirtualMod(IGame game, string name, IModDependencyList dependencies);
}