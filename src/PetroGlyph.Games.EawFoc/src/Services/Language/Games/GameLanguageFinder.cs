using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Search for installed game languages by analyzing installed game files, such as .meg archives.
/// </summary>
internal class GameLanguageFinder : IGameLanguageFinder
{
    private readonly ILanguageFinder _helper;

    /// <summary>
    /// Creates a instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public GameLanguageFinder(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));
        _helper = serviceProvider.GetRequiredService<ILanguageFinder>();
    }

    /// <inheritdoc/>
    public ISet<ILanguageInfo> FindInstalledLanguages(IGame game)
    {
        var text = _helper.GetTextLocalizations(game);
        var speech = _helper.GetSpeechLocalizationsFromMegs(game);
        var sfx = _helper.GetSfxMegLocalizations(game);
        return _helper.Merge(text, speech, sfx);
    }
}