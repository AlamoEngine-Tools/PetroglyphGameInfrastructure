using System.IO.Abstractions;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Services.Language;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;

#region Flags Normal

/// <summary>
/// Sets that the game instance will be in window mode.
/// </summary>
public sealed class WindowedArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public WindowedArgument() : base(ArgumentNameCatalog.WindowedArg, true)
    {
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class SafeMode() : FlagArgument(ArgumentNameCatalog.SafeModeArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class LowRamArgument() : FlagArgument(ArgumentNameCatalog.LowRamArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class MCEArgument() : FlagArgument(ArgumentNameCatalog.LowRamArg, true, true);

#endregion

#region Flags Debug

/// <summary>
/// The Debug build will ignore assert failures.
/// </summary>
public sealed class IgnoreAssertsArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public IgnoreAssertsArgument() : base(ArgumentNameCatalog.IgnoreAssertsArg, true)
    {
    }
}

/// <summary>
/// The Debug build will ignore assert failures.
/// </summary>
public sealed class NoArtProcessArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public NoArtProcessArgument() : base(ArgumentNameCatalog.NoArtProcessArg, true)
    {
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class RecordingArgument() : FlagArgument(ArgumentNameCatalog.RecordingArg, true);

/// <summary>
/// Indicates the game runs in multiple instances.
/// </summary>
public sealed class MultiArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public MultiArgument() : base(ArgumentNameCatalog.MultiArg, true)
    {
    }
}

/// <summary>
/// Saves code location of asserts into 'SavedAsserts.txt' next to the game.
/// </summary>
public sealed class PersistAssertsArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public PersistAssertsArgument() : base(ArgumentNameCatalog.PersistAssertsArg, true)
    {
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR! No Timeout when waiting for synchronization?!
/// </summary>
internal sealed class NoTimeoutArgument() : FlagArgument(ArgumentNameCatalog.NoTimeoutArg, true);

/// <summary>
/// Skips logging to files.
/// </summary>
public sealed class SuppressLogArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public SuppressLogArgument() : base(ArgumentNameCatalog.SuppressLogArg, true)
    {
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class PreValidateAudioMessagesArgument()
    : FlagArgument(ArgumentNameCatalog.PreValidateAudioMessagesArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class LeftLeftArgument() : FlagArgument(ArgumentNameCatalog.LeftLeftArg, true);

/// <summary>
/// Game uses deep synchronization.
/// </summary>
public sealed class DeppSyncArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public DeppSyncArgument() : base(ArgumentNameCatalog.DeepSyncArg, true)
    {
    }
}

/// <summary>
/// No Fog of War (FOW) is used.
/// </summary>
public sealed class NoFowArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public NoFowArgument() : base(ArgumentNameCatalog.NoFowArg, true)
    {
    }
}

/// <summary>
/// Skips intro videos.
/// </summary>
public sealed class NoIntroArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public NoIntroArgument() : base(ArgumentNameCatalog.NoIntroArg, true)
    {
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class SaveLoadDebugArgument() : FlagArgument(ArgumentNameCatalog.SaveLoadDebugArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class RefCountTrackingArgument() : FlagArgument(ArgumentNameCatalog.RefCountTrackingArg, true);

/// <summary>
/// Indicates that no physical mouse device is used.
/// </summary>
public sealed class NoHardwareMouseArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public NoHardwareMouseArgument() : base(ArgumentNameCatalog.NoHardwareMouseArg, true)
    {
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ProcessEnglishAssetsAndExitArgument()
    : FlagArgument(ArgumentNameCatalog.ProcessEnglishAssetsAndExitArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ProcessAssetsAndExitArgument() : FlagArgument(ArgumentNameCatalog.ProcessAssetsAndExitArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class AttractArgument() : FlagArgument(ArgumentNameCatalog.AttractArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class DebugUnitsArgument() : FlagArgument(ArgumentNameCatalog.DebugUnitsArg, true);

/// <summary>
/// Completely disables the user interface from the game.
/// </summary>
public sealed class NoMenuArgument : FlagArgument
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public NoMenuArgument() : base(ArgumentNameCatalog.NoMenuArg, true)
    {
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class FullScreenArgument() : FlagArgument(ArgumentNameCatalog.FullScreenArg, true);

#endregion

#region Normal

/// <summary>
/// The language which shall be applied to the game. If not present the system language will be used by the game.
/// </summary>
public sealed class LanguageArgument : NamedArgument<ILanguageInfo>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="language">The language of the game.</param>
    public LanguageArgument(ILanguageInfo language) : base(ArgumentNameCatalog.LanguageArg, language, false)
    {
    }

    /// <summary>
    /// Gets the english name of the language in upper-case.
    /// </summary>
    /// <returns></returns>
    public override string ValueToCommandLine()
    {
        return LanguageInfoUtilities.GetEnglishName(Value).ToUpperInvariant();
    }
}

/// <summary>
/// The local port number used by the game.
/// </summary>
public sealed class LocalPortArgument : NamedArgument<uint>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The local port to use.</param>
    public LocalPortArgument(uint value) : base(ArgumentNameCatalog.LocalPortArg, value, false)
    {
    }

    /// <summary>
    /// Gets the local port using for the game instance.
    /// </summary>
    /// <returns></returns>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// The system monitor number which shall be used for the game.
/// </summary>
public sealed class MonitorArgument : NamedArgument<uint>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The system monitor index to use.</param>
    public MonitorArgument(uint value) : base(ArgumentNameCatalog.MonitorArg, value, false)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// Game window width in pixels.
/// </summary>
public sealed class ScreenWidthArgument : NamedArgument<uint>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The window width in pixels.</param>
    public ScreenWidthArgument(uint value) : base(ArgumentNameCatalog.ScreenWidthArg, value, false)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// Game window height in pixels.
/// </summary>
public sealed class ScreenHeightArgument : NamedArgument<uint>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The window height in pixels.</param>
    public ScreenHeightArgument(uint value) : base(ArgumentNameCatalog.ScreenHeightArg, value, false)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// Limits the game to the given FPS ratio.
/// </summary>
public sealed class FPSCapArgument : NamedArgument<uint>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The max. FPS of the game.</param>
    public FPSCapArgument(uint value) : base(ArgumentNameCatalog.FPSCapArg, value, false)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

#endregion

#region Debug

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class OriginalAssetPathArgument(IDirectoryInfo value, IDirectoryInfo gameDirectory)
    : NamedArgument<IDirectoryInfo>(ArgumentNameCatalog.OriginalAssetPathArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().ShortenPath(Value, gameDirectory);
    }
}
    
/// <summary>
/// The initial seed value used for random number generation (RNG).
/// </summary>
public sealed class RandomSeedArgument : NamedArgument<uint>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The seed value.</param>
    public RandomSeedArgument(uint value) : base(ArgumentNameCatalog.RandomSeedArg, value, true)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ExpCDKeyArgument(string value)
    : NamedArgument<string>(ArgumentNameCatalog.ExpCDKeyArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return Value;
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class CDKeyArgument(string value) : NamedArgument<string>(ArgumentNameCatalog.CDKeyArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return Value;
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class MpPlaybackFileArgument(IFileInfo value, IDirectoryInfo gameDir)
    : NamedArgument<IFileInfo>(ArgumentNameCatalog.MPPlaybackFileArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().ShortenPath(Value, gameDir);
    }
}

/// <summary>
/// Loads the given map immediately after the game has started. 
/// </summary>
public sealed class MapArgument : NamedArgument<string>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The name of the map</param>
    public MapArgument(string value) : base(ArgumentNameCatalog.MapArg, value, true)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return Value;
    }
}

/// <summary>
/// Location where save game shall be stored.
/// <para>
/// On Windows non-absolute paths are
/// relative to %USERNAME%\Saved Games\Petroglyph\GAMETYPE
/// where GAMETYPE is either 'Empire At War' or 'Empire At War - Forces of Corruption'
/// </para>
/// </summary>
public sealed class SaveFolderArgument : NamedArgument<IDirectoryInfo>
{
    private readonly IDirectoryInfo _defaultRootSaveLocation;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The directory of save files.</param>
    /// <param name="defaultRootSaveLocation">The default root location.</param>
    public SaveFolderArgument(IDirectoryInfo value, IDirectoryInfo defaultRootSaveLocation) :
        base(ArgumentNameCatalog.MPPlaybackFileArg, value, true)
    {
        _defaultRootSaveLocation = defaultRootSaveLocation;
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().ShortenPath(Value, _defaultRootSaveLocation);
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class QuickLoadRecordingArgument(string value)
    : NamedArgument<string>(ArgumentNameCatalog.QuickLoadRecordingArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return Value;
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class QuickLoadArgument(string value)
    : NamedArgument<string>(ArgumentNameCatalog.QuickLoadArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return Value;
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ConfigArgument(string value) : NamedArgument<string>(ArgumentNameCatalog.ConfigArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return Value;
    }
}

/// <summary>
/// Specifies the profile this game shall start with.
/// <para>
/// Creates a new profile if the given profile does not exists.
/// </para>
/// <para>On Windows profiles are persisted in the registry at HKCU\software\petroglyph\GAMETYPE\Profiles
/// where GAMETYPE is either 'StarWars' or 'StarWars FOC'.</para>
/// </summary>
public sealed class ProfileArgument : NamedArgument<uint>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The profile number.</param>
    public ProfileArgument(uint value) : base(ArgumentNameCatalog.ProfileArg, value, true)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// Broadcasting port to listen to messages from other game instances.
/// </summary>
/// <remarks>The value usually matches with <see cref="LocalPortArgument"/> from the other instance.</remarks>
public sealed class BCast2Argument : NamedArgument<uint>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The port number</param>
    public BCast2Argument(uint value) : base(ArgumentNameCatalog.BCast2Arg, value, true)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class BCast3Argument(uint value) : NamedArgument<uint>(ArgumentNameCatalog.BCast3Arg, value, true)
{
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class BCast4Argument(uint value) : NamedArgument<uint>(ArgumentNameCatalog.BCast4Arg, value, true)
{
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// Specifies the level of detail for LUA logging.
/// </summary>
public sealed class AILogStyleArgument : NamedArgument<AILogStyleArgument.AILogStyle>
{
    /// <summary>
    /// Level of detail of LUA logging.
    /// </summary>
    [SerializeEnumName]
    public enum AILogStyle
    {
        /// <summary>
        /// LUA logging is disabled.
        /// </summary>
        None,
        /// <summary>
        /// Default LUA logging level.
        /// </summary>
        Normal,
        /// <summary>
        /// Verbose LUA logging level.
        /// </summary>
        Heavy
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The logging style to use.</param>
    public AILogStyleArgument(AILogStyle value) : base(ArgumentNameCatalog.AILogStyleArg, value, true)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value).ToUpperInvariant();
    }
}

/// <summary>
/// File location where LUA logging shall be stored to.
/// </summary>
/// <remarks>
/// The directory of the file must already exists.
/// </remarks>
public sealed class AILogFileArgument : NamedArgument<IFileInfo>
{
    private readonly IDirectoryInfo _gameDir;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The log file.</param>
    /// <param name="gameDir">The game's directory.</param>
    public AILogFileArgument(IFileInfo value, IDirectoryInfo gameDir) : base(ArgumentNameCatalog.AILogFileArg, value, true)
    {
        _gameDir = gameDir;
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().ShortenPath(Value, _gameDir);
    }
}

/// <summary>
/// File location where RNG logging shall be stored to.
/// </summary>
/// <remarks>
/// The directory of the file must already exists.
/// </remarks>
public sealed class RandomLogFileArgument : NamedArgument<IFileInfo>
{
    private readonly IDirectoryInfo _gameDir;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The log file.</param>
    /// <param name="gameDir">The game's directory.</param>
    public RandomLogFileArgument(IFileInfo value, IDirectoryInfo gameDir) : base(ArgumentNameCatalog.RandomLogFileArg, value, true)
    {
        _gameDir = gameDir;
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().ShortenPath(Value, _gameDir);
    }
}

/// <summary>
/// File location where logging shall be stored to.
/// </summary>
/// <remarks>
/// The directory of the file must already exists.
/// </remarks>
public sealed class LogFileArgument : NamedArgument<IFileInfo>
{
    private readonly IDirectoryInfo _gameDir;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The log file.</param>
    /// <param name="gameDir">The game's directory.</param>
    public LogFileArgument(IFileInfo value, IDirectoryInfo gameDir) : base(ArgumentNameCatalog.LogFileArg, value, true)
    {
        _gameDir = gameDir;
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().ShortenPath(Value, _gameDir);
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ConsoleCommandFileArgument(IFileInfo value, IDirectoryInfo gameDir)
    : NamedArgument<IFileInfo>(ArgumentNameCatalog.ConsoleCommandFileArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().ShortenPath(Value, gameDir);
    }
}

/// <summary>
/// Logging filter for events relevant for multiplayer synchronization. Default value is 15. 
/// </summary>
/// <remarks>This obviously is an enum but it's not documented, so we leave it as a plain ushort value.</remarks>
public sealed class SyncLogFilterArgument : NamedArgument<ushort>
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="value">The log filter value.</param>
    public SyncLogFilterArgument(ushort value) : base(ArgumentNameCatalog.SyncLogFilterArg, value, true)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR! Not sure it 'connects' to.
/// </summary>
internal sealed class ConnectPortArgument(uint value)
    : NamedArgument<uint>(ArgumentNameCatalog.ConnectPortArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR! Not sure it 'connects' to.
/// </summary>
internal sealed class ConnectIPArgument(string value)
    : NamedArgument<string>(ArgumentNameCatalog.ConnectIPArg, value, true)
{
    public override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
    }
}
#endregion