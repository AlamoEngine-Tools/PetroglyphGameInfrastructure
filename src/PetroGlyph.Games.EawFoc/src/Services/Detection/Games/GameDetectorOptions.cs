using System;
using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Query options for game detections. 
/// </summary>
/// <remarks>If no <see cref="TargetPlatforms"/> is explicitly specified,
/// the query will contain <see cref="GamePlatform.Undefined"/>, which means that any found platform shall match.</remarks>
/// <param name="Type">The game type which shall be searched.</param>
public sealed record GameDetectorOptions(GameType Type)
{
    private static readonly GamePlatform[] AnyPlatform = [GamePlatform.Undefined];

    private IList<GamePlatform> _targetPlatforms = AnyPlatform;

    /// <summary>
    /// Prioritized list of platforms the search query has to match. Default is <see cref="GamePlatform.Undefined"/>
    /// </summary>
    public IList<GamePlatform> TargetPlatforms
    {
        get => _targetPlatforms;
        set => _targetPlatforms = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal GameDetectorOptions Normalize()
    {
        if (!TargetPlatforms.Any())
            return this with { TargetPlatforms = new List<GamePlatform> { GamePlatform.Undefined } };
        if (TargetPlatforms.Contains(GamePlatform.Undefined))
            return this with { TargetPlatforms = AnyPlatform };
        return this with { TargetPlatforms = TargetPlatforms.Distinct().ToList() };
    }
}