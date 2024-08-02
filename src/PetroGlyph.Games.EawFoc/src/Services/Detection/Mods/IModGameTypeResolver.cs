using System.IO.Abstractions;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// 
/// </summary>
public interface IModGameTypeResolver
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="modType"></param>
    /// <param name="searchModInfo"></param>
    /// <param name="gameType"></param>
    /// <returns></returns>
    public bool TryGetGameType(IDirectoryInfo directory, ModType modType, bool searchModInfo, out GameType gameType);


    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="modType"></param>
    /// <param name="modinfo"></param>
    /// <param name="gameType"></param>
    /// <returns></returns>
    public bool TryGetGameType(IDirectoryInfo directory, ModType modType, IModinfo? modinfo, out GameType gameType);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modType"></param>
    /// <param name="modinfo"></param>
    /// <param name="gameType"></param>
    /// <returns></returns>
    public bool TryGetGameType(ModType modType, IModinfo modinfo, out GameType gameType);

}