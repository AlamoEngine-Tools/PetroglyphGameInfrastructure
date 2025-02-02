using System;
using AET.Modinfo.Spec;
using AET.Modinfo.Utilities;
using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Service to determine a <see cref="GameType"/> from mod information.
/// </summary>
public interface IModGameTypeResolver
{
    /// <summary>
    /// Tries to determine the <see cref="GameType"/> from a specified mod location.
    /// </summary>
    /// <remarks>
    /// This method only returns <see langword="true"/>, if there is clear evidence a mod is associated to a specific game type. 
    /// </remarks>
    /// <param name="modInformation">The information of the mod.</param>
    /// <param name="gameTypes">When this method returns <see langword="true"/>, the assured game types will be stored in this variable.</param>
    /// <returns><see langword="true"/> when the game type could be determined; <see langword="false"/> if there is no clear evidence of the actual game type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="modInformation"/> is <see langword="null"/>.</exception>
    public bool TryGetGameType(DetectedModReference modInformation, out ReadOnlyFrugalList<GameType> gameTypes);

    /// <summary>
    /// Tries to determine the <see cref="GameType"/> from a specified modinfo data.
    /// </summary>
    /// <remarks>
    /// This method only returns <see langword="true"/>, if there is clear evidence a mod is associated to a specific game type. 
    /// </remarks>
    /// <param name="modinfo">The modinfo data.</param>
    /// <param name="gameTypes">When this method returns <see langword="true"/>, the assured game types will be stored in this variable.</param>
    /// <returns><see langword="true"/> when the game type could be determined; <see langword="false"/> if there is no clear evidence of the actual game type.</returns>
    public bool TryGetGameType(IModinfo? modinfo, out ReadOnlyFrugalList<GameType> gameTypes);

    /// <summary>
    /// Determines whether the specified mod information are definitely not compatible to a game type.
    /// </summary>
    /// <param name="modInformation">The information of the mod.</param>
    /// <param name="expectedGameType">The expectedGameType game type the mod must be compatible with.</param>
    /// <returns>
    /// <see langword="true"/> only if <paramref name="modInformation"/> is definitely not compatible with <paramref name="expectedGameType"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="modInformation"/> is <see langword="null"/>.</exception>
    public bool IsDefinitelyNotCompatibleToGame(DetectedModReference modInformation, GameType expectedGameType);

    /// <summary>
    /// Determines whether the specified mod information are definitely not compatible to a game type.
    /// </summary>
    /// <param name="modinfo">The modinfo data.</param>
    /// <param name="expectedGameType">The expectedGameType game type the mod must be compatible with.</param>
    /// <returns>
    /// <see langword="true"/> only if <paramref name="modinfo"/> is definitely not compatible with <paramref name="expectedGameType"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsDefinitelyNotCompatibleToGame(IModinfo? modinfo, GameType expectedGameType);
}