﻿using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Language;

/// <summary>
/// Search for installed game languages by analyzing installed game files, such as .meg archives.
/// </summary>
public class GameLanguageFinder : IGameLanguageFinder
{
    private readonly ILanguageFinder _helper;

    /// <summary>
    /// Creates a instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public GameLanguageFinder(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _helper = serviceProvider.GetService<ILanguageFinder>() ?? new FileBasedLanguageFinder();
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