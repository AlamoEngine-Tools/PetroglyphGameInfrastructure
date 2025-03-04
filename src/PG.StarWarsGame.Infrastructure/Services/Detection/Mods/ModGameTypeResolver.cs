using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;
using AET.Modinfo.Spec.Steam;
using AET.Modinfo.Utilities;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Base class for an <see cref="IModGameTypeResolver"/>.
/// </summary>
public abstract class ModGameTypeResolver : IModGameTypeResolver
{
    /// <summary>
    /// The service provider.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider;

    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogger? Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModGameTypeResolver"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    protected ModGameTypeResolver(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    /// <inheritdoc />
    public bool TryGetGameType(DetectedModReference modInformation, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        if (modInformation == null)
            throw new ArgumentNullException(nameof(modInformation));
        Logger?.LogTrace($"Try getting game type for '{modInformation.ModReference.Identifier}'");
        var result = TryGetGameTypeCore(modInformation, out gameTypes);
        Logger?.LogDebug($"Detected game types '{modInformation.ModReference.Identifier}': Success: {result}; Types: [{string.Join("", gameTypes)}]");
        return result;
    }

    /// <summary>
    /// Tries to determine the <see cref="GameType"/> from a specified mod location.
    /// </summary>
    /// <remarks>
    /// This method only returns <see langword="true"/>, if there is clear evidence a mod is associated to a specific game type. 
    /// </remarks>
    /// <param name="modInformation">The information of the mod.</param>
    /// <param name="gameTypes">When this method returns <see langword="true"/>, the assured game types will be stored in this variable.</param>
    /// <returns><see langword="true"/> when the game type could be determined; <see langword="false"/> if there is no clear evidence of the actual game type.</returns>
    protected internal abstract bool TryGetGameTypeCore(DetectedModReference modInformation, out ReadOnlyFrugalList<GameType> gameTypes);

    /// <inheritdoc />
    public virtual bool TryGetGameType(IModinfo? modinfo, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        return GetGameType(modinfo?.SteamData, out gameTypes);
    }

    /// <inheritdoc />
    public bool IsDefinitelyNotCompatibleToGame(DetectedModReference modInformation, GameType expectedGameType)
    {
        Logger?.LogTrace($"Checking if '{modInformation.ModReference.Identifier}' is not compatible to {expectedGameType}.");
        // If the type resolver was unable to find the type, we have to assume that the current mod matches to the game.
        // Otherwise, we'd produce false negatives. Only if the resolver was able to determine a result, we use that finding.
        return TryGetGameType(modInformation, out var gameTypes) && !gameTypes.Contains(expectedGameType);
    }

    /// <inheritdoc />
    public bool IsDefinitelyNotCompatibleToGame(IModinfo? modinfo, GameType expectedGameType)
    {
        Logger?.LogTrace($"Checking if modinfo '{modinfo?.Name}' is not compatible to {expectedGameType}.");
        // If the type resolver was unable to find the type, we have to assume that the current mod matches to the game.
        // Otherwise, we'd produce false negatives. Only if the resolver was able to determine a result, we use that finding.
        return TryGetGameType(modinfo, out var gameTypes) && !gameTypes.Contains(expectedGameType);
    }

    private bool GetGameType(ISteamData? steamData, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        gameTypes = default;
        Logger?.LogTrace($"Try getting game type from steam data '[{string.Join(",", steamData?.Tags ?? ["tags n/a"])}]'");
        return steamData is not null && GetGameTypesFromTags(steamData.Tags, out gameTypes);
    }

    internal static bool GetGameTypesFromTags(IEnumerable<string> tags, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        var mutableGameTypes = new FrugalList<GameType>();

        foreach (var tag in tags)
        {
            var trimmed = tag.AsSpan().Trim();

            if (trimmed.Equals("EAW".AsSpan(), StringComparison.OrdinalIgnoreCase)) 
                mutableGameTypes.Add(GameType.Eaw);

            if (trimmed.Equals("FOC".AsSpan(), StringComparison.OrdinalIgnoreCase)) 
                mutableGameTypes.Add(GameType.Foc);
        }

        gameTypes = mutableGameTypes.AsReadOnly();
        return gameTypes.Count > 0;
    }
}