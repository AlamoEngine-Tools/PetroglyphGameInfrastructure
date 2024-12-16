using System;
using System.Globalization;
using System.IO;
using EawModinfo;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services;

/// <summary>
/// Factory class to create mods for a Petroglyph Star Wars game.
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
    /// <exception cref="ArgumentNullException"><paramref name="game"/> or <paramref name="modReference"/> or <paramref name="culture"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="modReference"/> is references a virtual mod.</exception>
    /// <exception cref="DirectoryNotFoundException"> The directory information of <paramref name="modReference"/> does not exist.</exception>
    /// <exception cref="ModException">
    /// <paramref name="modReference"/> is not compatible to <paramref name="game"/>
    /// OR
    /// It is not possible to create a mod instance because the information processed from <paramref name="modReference"/> are invalid.
    /// </exception>
    /// <exception cref="ModinfoException">The modinfo data in of <paramref name="modReference"/>, if present, is not valid.</exception>
    IPhysicalMod CreatePhysicalMod(IGame game, DetectedModReference modReference, CultureInfo culture);

    /// <summary>
    /// Creates a virtual mods from a given <see cref="IModinfo"/>.
    /// </summary>
    /// <remarks>The created mods do NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
    /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
    /// <param name="virtualModInfo">Set of <see cref="IModinfo"/> where each element defines its own virtual mod.</param>
    /// <returns>One or many virtual mods</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> or <paramref name="virtualModInfo"/> is <see langword="null"/>.</exception>
    /// <exception cref="ModinfoException"><paramref name="virtualModInfo"/> is not valid.</exception>
    /// <exception cref="ModException"><paramref name="virtualModInfo"/> has invalid dependency information to create a virtual mod.</exception>
    IVirtualMod CreateVirtualMod(IGame game, IModinfo virtualModInfo);

    /// <summary>
    /// Creates virtual mods for a game.
    /// </summary>
    /// <remarks>The created mods dot NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
    /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
    /// <param name="name">The name of the virtual mod.</param>
    /// <param name="dependencies">The dependencies of the virtual mod.</param>
    /// <returns>One or many virtual mods</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> or <paramref name="name"/> or <paramref name="dependencies"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    /// <exception cref="ModException"><paramref name="dependencies"/> has invalid information to create a virtual mod.</exception>
    IVirtualMod CreateVirtualMod(IGame game, string name, IModDependencyList dependencies);
}