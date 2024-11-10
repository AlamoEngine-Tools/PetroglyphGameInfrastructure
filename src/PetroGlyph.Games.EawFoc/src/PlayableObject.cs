using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// Base class for <see cref="IPlayableObject"/> which supports one-time initialization
/// for <see cref="IPlayableObject.IconFile"/> and <see cref="IPlayableObject.InstalledLanguages"/>
/// including a reset option, to re-initialize.
/// </summary>
public abstract class PlayableObject(IServiceProvider serviceProvider) : IPlayableObject
{
    private IReadOnlyCollection<ILanguageInfo>? _installedLanguages;
    private string? _iconFile;

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
                return _installedLanguages!;
            if (_installedLanguages is not null)
                return _installedLanguages;
            _installedLanguages = ResolveInstalledLanguages();
            if (_installedLanguages is null)
                throw new PetroglyphException("Resolved languages must not be null.");
            _languageSearched = true;
            return _installedLanguages;
        }
    }

    /// <inheritdoc/>
    public string? IconFile
    {
        get
        {
            if (_iconSearched)
                return _iconFile;
            if (_iconFile is not null)
                return _iconFile;
            _iconFile = ResolveIconFile();
            _iconSearched = true;
            return _iconFile;
        }
    }

    /// <summary>
    /// Initialization function to resolve a icon file. Default implements returns <see langword="null"/>.
    /// </summary>
    /// <returns>The resolved relative or absolute icon path, or <see langword="null"/>.</returns>
    protected virtual string? ResolveIconFile()
    {
        return null;
    }

    /// <summary>
    /// Initialization function to resolve installed languages. Default implements returns an empty set.
    /// </summary>
    /// <returns></returns>
    protected virtual IReadOnlyCollection<ILanguageInfo> ResolveInstalledLanguages()
    {
        return new HashSet<ILanguageInfo>();
    }
}