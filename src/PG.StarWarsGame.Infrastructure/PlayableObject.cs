using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Icon;
using PG.StarWarsGame.Infrastructure.Services.Language;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// Base class for <see cref="IPlayableObject"/> which supports one-time initialization
/// for <see cref="IPlayableObject.IconFile"/> and <see cref="IPlayableObject.InstalledLanguages"/>
/// including a reset option, to re-initialize.
/// </summary>
public abstract class PlayableObject(IServiceProvider serviceProvider) : IPlayableObject
{
    private bool _languageSearched;
    private bool _iconSearched;

    /// <summary>
    /// The service provider.
    /// </summary>
    protected IServiceProvider ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract IGame Game { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<ILanguageInfo> InstalledLanguages
    {
        get
        {
            if (_languageSearched)
                return field!;
            field = ResolveInstalledLanguages();
            if (field is null)
                throw new PetroglyphException("Resolved languages must not be null.");
            _languageSearched = true;
            return field;
        }
    }

    /// <inheritdoc/>
    public string? IconFile
    {
        get
        {
            if (_iconSearched)
                return field;
            field = ResolveIconFile();
            _iconSearched = true;
            return field;
        }
    }

    private string? ResolveIconFile()
    {
        return ServiceProvider.GetRequiredService<IIconFinder>().FindIcon(this);
    }

    private IReadOnlyCollection<ILanguageInfo> ResolveInstalledLanguages()
    {
        return ServiceProvider.GetRequiredService<ILanguageFinder>().FindLanguages(this);
    }
}