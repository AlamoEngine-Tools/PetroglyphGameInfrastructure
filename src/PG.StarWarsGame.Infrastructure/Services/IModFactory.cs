using System;
using System.Globalization;
using System.IO;
using AET.Modinfo;
using AET.Modinfo.Spec;
using AET.Modinfo.Utilities;
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
    /// <param name="detectedMod">The mod reference to use for creating the mod instance.</param>
    /// <param name="culture">The culture that shall be used to determine the mod's name.</param>
    /// <returns>Teh created mod instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> or <paramref name="detectedMod"/> or <paramref name="culture"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="detectedMod"/> is references a virtual mod.</exception>
    /// <exception cref="DirectoryNotFoundException"> The directory information of <paramref name="detectedMod"/> does not exist.</exception>
    /// <exception cref="ModException">
    /// <paramref name="detectedMod"/> is not compatible to <paramref name="game"/>
    /// OR
    /// The resolved name of the mod is null or empty.
    /// </exception>
    /// <exception cref="ModinfoException">The modinfo data in of <paramref name="detectedMod"/>, if present, is not valid.</exception>
    IPhysicalMod CreatePhysicalMod(IGame game, DetectedModReference detectedMod, CultureInfo culture);

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
}