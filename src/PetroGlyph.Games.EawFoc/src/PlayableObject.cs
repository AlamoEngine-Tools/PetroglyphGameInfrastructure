using System.Collections.Generic;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc;

/// <summary>
/// Base class for <see cref="IPlayableObject"/> which supports one-time initialization
/// for <see cref="IPlayableObject.IconFile"/> and <see cref="IPlayableObject.InstalledLanguages"/>
/// including a reset option, to re-initialize.
/// </summary>
public abstract class PlayableObject : IPlayableObject
{
    private ISet<ILanguageInfo>? _installedLanguages;
    private string? _iconFile;

    private bool _languageSearched;
    private bool _iconSearched;

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public ISet<ILanguageInfo> InstalledLanguages
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
    protected virtual ISet<ILanguageInfo> ResolveInstalledLanguages()
    {
        return new HashSet<ILanguageInfo>();
    }

    /// <summary>
    /// Resets the state of <see cref="IconFile"/> so it can be resolved again.
    /// </summary>
    /// <returns>Returns the old value.</returns>
    public virtual string? ResetIcon()
    {
        var oldIcon = _iconFile;
        _iconFile = null;
        _iconSearched = false;
        return oldIcon;
    }

    /// <summary>
    /// Resets the state of <see cref="InstalledLanguages"/> so it can be resolved again.
    /// </summary>
    /// <remarks>Return value can be <see langword="null"/>, if <see cref="InstalledLanguages"/> was not called at least once before resetting.</remarks>
    /// <returns>Returns the old value.</returns>
    public virtual ISet<ILanguageInfo>? ResetLanguages()
    {
        var oldLanguages = _installedLanguages;
        _installedLanguages = null;
        _languageSearched = false;
        return oldLanguages;
    }
}