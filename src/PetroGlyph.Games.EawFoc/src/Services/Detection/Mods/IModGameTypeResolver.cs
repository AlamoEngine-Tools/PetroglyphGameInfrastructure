using System.IO.Abstractions;
using EawModinfo.Spec;
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
    /// <param name="modLocation">The location of the mod.</param>
    /// <param name="modType">The type of the mod.</param>
    /// <param name="modinfo">Optional modinfo data to consider.</param>
    /// <param name="gameType">When this method return <see langword="true"/>, the found game type will be stored in this variable.</param>
    /// <returns><see langword="true"/> when the game type could be determined; <see langword="false"/> if there is no clear evidence of the actual game type.</returns>
    public bool TryGetGameType(IDirectoryInfo modLocation, ModType modType, IModinfo? modinfo, out GameType gameType);

    /// <summary>
    /// Tries to determine the <see cref="GameType"/> from a specified modinfo data.
    /// </summary>
    /// <remarks>
    /// This method only returns <see langword="true"/>, if there is clear evidence a mod is associated to a specific game type. 
    /// </remarks>
    /// <param name="modinfo">The modinfo data.</param>
    /// <param name="gameType">When this method return <see langword="true"/>, the found game type will be stored in this variable.</param>
    /// <returns><see langword="true"/> when the game type could be determined; <see langword="false"/> if there is no clear evidence of the actual game type.</returns>
    public bool TryGetGameType(IModinfo? modinfo, out GameType gameType);
}