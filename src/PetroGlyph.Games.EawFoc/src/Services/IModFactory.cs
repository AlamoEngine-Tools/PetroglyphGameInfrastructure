using System.Collections.Generic;
using System.Globalization;
using System.IO;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Services;

/// <summary>
/// Factory to create one or more <see cref="IMod"/>
/// </summary>
public interface IModFactory
{
    /// <summary>
    /// Creates a new <see cref="IPhysicalMod"/> instance for a game from a file system path.
    /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
    /// </summary>
    /// <remarks>The created mods do NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
    /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
    /// <param name="modReference">The mod reference of the new mod.</param>
    /// <param name="culture">The culture that shall be used to determine the mod's name.</param>
    /// <returns>the Mod instance</returns>
    /// <exception cref="DirectoryNotFoundException">when <paramref name="modReference"/> could not be located as an existing location</exception>
    /// <exception cref="ModException">
    /// <paramref name="modReference"/> or identified modinfo files had illegal information, such as empty names.
    /// </exception>
    IMod FromReference(IGame game, DetectedModReference modReference, CultureInfo culture);

    /// <summary>
    /// Creates a new <see cref="IPhysicalMod"/> instance for a game from a file system path.
    /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
    /// </summary>
    /// <remarks>The created mods do NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
    /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
    /// <param name="modReference">The mod reference of the new mod.</param>
    /// <param name="modinfo">Optional <see cref="IModinfo"/> from which the mod will get initialized.</param>
    /// <param name="culture">The culture that shall be used to determine the mod's name.</param>
    /// <returns>the Mod instance</returns>
    /// <exception cref="DirectoryNotFoundException">when <paramref name="modReference"/> could not be located as an existing location</exception>
    /// <exception cref="ModException">
    /// <paramref name="modReference"/> or identified modinfo files had illegal information, such as empty names.
    /// </exception>
    IMod FromReference(IGame game, IModReference modReference, IModinfo? modinfo, CultureInfo culture);

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
    /// <param name="dependencies">dependencies of the mod.
    /// The value are the sorted dependencies of the virtual mod</param>
    /// <param name="resolveLayout">The resolve layout of the <paramref name="dependencies"/> list.</param>
    /// <returns>One or many virtual mods</returns>
    /// <exception cref="PetroglyphException">if the virtual mod could not be created.</exception>
    IVirtualMod CreateVirtualMod(IGame game, string name, IList<ModDependencyEntry> dependencies, DependencyResolveLayout resolveLayout);
}