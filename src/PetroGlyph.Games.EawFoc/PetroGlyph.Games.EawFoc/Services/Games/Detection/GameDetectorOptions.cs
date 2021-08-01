using System.Collections.Generic;
using System.Linq;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <summary>
    /// Query options for game detections. 
    /// </summary>
    /// <remarks>If no <see cref="TargetPlatforms"/> is explicitly specified,
    /// the query will contain <see cref="GamePlatform.Undefined"/>, which means that any found platform shall match.</remarks>
    /// <param name="Type">The game type which shall be searched.</param>
    public record GameDetectorOptions(GameType Type)
    {
        private static readonly GamePlatform[] AnyPlatform = { GamePlatform.Undefined };

        private IList<GamePlatform> _targetPlatforms = AnyPlatform;

        /// <summary>
        /// Prioritized list of platforms the search query has to match. Default is <see cref="GamePlatform.Undefined"/>
        /// </summary>
        public IList<GamePlatform> TargetPlatforms
        {
            get => _targetPlatforms;
            set
            {
                Requires.NotNull(value, nameof(value));
                _targetPlatforms = value;
            }
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
}