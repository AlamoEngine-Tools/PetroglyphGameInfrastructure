using System.IO.Abstractions;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using PG.StarWarsGame.Infrastructure.Services.Language;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;

#region Flags Normal

/// <summary>
/// Sets that the game instance will be in window mode.
/// </summary>
public sealed class WindowedArgument() : FlagArgument(GameArgumentNames.WindowedArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class SafeMode() : FlagArgument(GameArgumentNames.SafeModeArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class LowRamArgument() : FlagArgument(GameArgumentNames.LowRamArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class MCEArgument() : FlagArgument(GameArgumentNames.MCEArg, true, true);

#endregion

#region Flags Debug

/// <summary>
/// The Debug build will ignore assert failures.
/// </summary>
public sealed class IgnoreAssertsArgument() : FlagArgument(GameArgumentNames.IgnoreAssertsArg, true);

/// <summary>
/// The Debug build will ignore assert failures.
/// </summary>
public sealed class NoArtProcessArgument() : FlagArgument(GameArgumentNames.NoArtProcessArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class RecordingArgument() : FlagArgument(GameArgumentNames.RecordingArg, true);

/// <summary>
/// Indicates the game runs in multiple instances.
/// </summary>
public sealed class MultiArgument() : FlagArgument(GameArgumentNames.MultiArg, true);

/// <summary>
/// Saves code location of asserts into 'SavedAsserts.txt' next to the game.
/// </summary>
public sealed class PersistAssertsArgument() : FlagArgument(GameArgumentNames.PersistAssertsArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR! No Timeout when waiting for synchronization?!
/// </summary>
internal sealed class NoTimeoutArgument() : FlagArgument(GameArgumentNames.NoTimeoutArg, true);

/// <summary>
/// Skips logging to files.
/// </summary>
public sealed class SuppressLogArgument() : FlagArgument(GameArgumentNames.SuppressLogArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class PreValidateAudioMessagesArgument() : FlagArgument(GameArgumentNames.PreValidateAudioMessagesArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class LeftLeftArgument() : FlagArgument(GameArgumentNames.LeftLeftArg, true);

/// <summary>
/// Game uses deep synchronization.
/// </summary>
public sealed class DeppSyncArgument() : FlagArgument(GameArgumentNames.DeepSyncArg, true);

/// <summary>
/// No Fog of War (FOW) is used.
/// </summary>
public sealed class NoFowArgument() : FlagArgument(GameArgumentNames.NoFowArg, true);

/// <summary>
/// Skips intro videos.
/// </summary>
public sealed class NoIntroArgument() : FlagArgument(GameArgumentNames.NoIntroArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class SaveLoadDebugArgument() : FlagArgument(GameArgumentNames.SaveLoadDebugArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class RefCountTrackingArgument() : FlagArgument(GameArgumentNames.RefCountTrackingArg, true);

/// <summary>
/// Indicates that no physical mouse device is used.
/// </summary>
public sealed class NoHardwareMouseArgument() : FlagArgument(GameArgumentNames.NoHardwareMouseArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ProcessEnglishAssetsAndExitArgument() : FlagArgument(GameArgumentNames.ProcessEnglishAssetsAndExitArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ProcessAssetsAndExitArgument() : FlagArgument(GameArgumentNames.ProcessAssetsAndExitArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class AttractArgument() : FlagArgument(GameArgumentNames.AttractArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class DebugUnitsArgument() : FlagArgument(GameArgumentNames.DebugUnitsArg, true);

/// <summary>
/// Completely disables the user interface from the game.
/// </summary>
public sealed class NoMenuArgument() : FlagArgument(GameArgumentNames.NoMenuArg, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class FullScreenArgument() : FlagArgument(GameArgumentNames.FullScreenArg, true);

#endregion

#region Normal

/// <summary>
/// The language which shall be applied to the game. If not present the system language will be used by the game.
/// </summary>
public sealed class LanguageArgument(ILanguageInfo language) : NamedArgument<ILanguageInfo>(GameArgumentNames.LanguageArg, language, false)
{
    internal override string ValueToCommandLine()
    {
        return LanguageInfoUtilities.GetEnglishName(Value)!.ToUpperInvariant();
    }
}

/// <summary>
/// The local port number used by the game.
/// </summary>
public sealed class LocalPortArgument(uint value) : NamedArgument<uint>(GameArgumentNames.LocalPortArg, value, false);

/// <summary>
/// The system monitor number which shall be used for the game.
/// </summary>
public sealed class MonitorArgument(uint value) : NamedArgument<uint>(GameArgumentNames.MonitorArg, value, false);

/// <summary>
/// Game window width in pixels.
/// </summary>
public sealed class ScreenWidthArgument(uint value) : NamedArgument<uint>(GameArgumentNames.ScreenWidthArg, value, false);

/// <summary>
/// Game window height in pixels.
/// </summary>
public sealed class ScreenHeightArgument(uint value) : NamedArgument<uint>(GameArgumentNames.ScreenHeightArg, value, false);

/// <summary>
/// Limits the game to the given FPS ratio.
/// </summary>
public sealed class FPSCapArgument(uint value) : NamedArgument<uint>(GameArgumentNames.FPSCapArg, value, false);

#endregion

#region Debug

/// <summary>
/// Sets a fallback path where game assets, such as .MEG files, are located.
/// </summary>
public sealed class OriginalAssetPathArgument(IDirectoryInfo assetDir, IDirectoryInfo gameDirectory)
    : NamedArgument<IDirectoryInfo>(GameArgumentNames.OriginalAssetPathArg, assetDir, true)
{
    internal override bool HasPathValue => true;

    internal override string ValueToCommandLine()
    {
        return ArgumentValueSerializer.ShortenPath(Value, gameDirectory);
    }
}

/// <summary>
/// The initial seed value used for random number generation (RNG).
/// </summary>
public sealed class RandomSeedArgument(uint value) : NamedArgument<uint>(GameArgumentNames.RandomSeedArg, value, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ExpCDKeyArgument(string value) : NamedArgument<string>(GameArgumentNames.ExpCDKeyArg, value, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class CDKeyArgument(string value) : NamedArgument<string>(GameArgumentNames.CDKeyArg, value, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class MpPlaybackFileArgument(IFileInfo value, IDirectoryInfo gameDir)
    : NamedArgument<IFileInfo>(GameArgumentNames.MPPlaybackFileArg, value, true)
{
    internal override bool HasPathValue => true;

    internal override string ValueToCommandLine()
    {
        return ArgumentValueSerializer.ShortenPath(Value, gameDir);
    }
}

/// <summary>
/// Loads the given map immediately after the game has started. 
/// </summary>
public sealed class MapArgument(string value) : NamedArgument<string>(GameArgumentNames.MapArg, value, true);

/// <summary>
/// Location where save game shall be stored.
/// <para>
/// On Windows non-absolute paths are
/// relative to %USERNAME%\Saved Games\Petroglyph\GAMETYPE
/// where GAMETYPE is either 'Empire At War' or 'Empire At War - Forces of Corruption'
/// </para>
/// </summary>
public sealed class SaveFolderArgument(IDirectoryInfo value, IDirectoryInfo defaultRootSaveLocation) 
    : NamedArgument<IDirectoryInfo>(GameArgumentNames.MPPlaybackFileArg, value, true)
{
    internal override bool HasPathValue => true;

    internal override string ValueToCommandLine()
    {
        return ArgumentValueSerializer.ShortenPath(Value, defaultRootSaveLocation);
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class QuickLoadRecordingArgument(string value) : NamedArgument<string>(GameArgumentNames.QuickLoadRecordingArg, value, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class QuickLoadArgument(string value) : NamedArgument<string>(GameArgumentNames.QuickLoadArg, value, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ConfigArgument(string value) : NamedArgument<string>(GameArgumentNames.ConfigArg, value, true);

/// <summary>
/// Specifies the profile this game shall start with.
/// <para>
/// Creates a new profile if the given profile does not exist.
/// </para>
/// <para>On Windows profiles are persisted in the registry at HKCU\software\petroglyph\GAMETYPE\Profiles
/// where GAMETYPE is either 'StarWars' or 'StarWars FOC'.</para>
/// </summary>
public sealed class ProfileArgument(uint value) : NamedArgument<uint>(GameArgumentNames.ProfileArg, value, true);

/// <summary>
/// Broadcasting port to listen to messages from other game instances.
/// </summary>
/// <remarks>The value usually matches with <see cref="LocalPortArgument"/> from the other instance.</remarks>
public sealed class BCast2Argument(uint value) : NamedArgument<uint>(GameArgumentNames.BCast2Arg, value, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class BCast3Argument(uint value) : NamedArgument<uint>(GameArgumentNames.BCast3Arg, value, true);

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class BCast4Argument(uint value) : NamedArgument<uint>(GameArgumentNames.BCast4Arg, value, true);

/// <summary>
/// Specifies the level of detail for LUA logging.
/// </summary>
public sealed class AILogStyleArgument(AILogStyle value) 
    : NamedArgument<AILogStyle>(GameArgumentNames.AILogStyleArg, value, true)
{
    internal override string ValueToCommandLine()
    {
        return ArgumentValueSerializer.Serialize(Value).ToUpperInvariant();
    }
}

/// <summary>
/// File location where LUA logging shall be stored to.
/// </summary>
/// <remarks>
/// The directory of the file must already exists.
/// </remarks>
public sealed class AILogFileArgument(IFileInfo value, IDirectoryInfo gameDir) : NamedArgument<IFileInfo>(GameArgumentNames.AILogFileArg, value, true)
{
    internal override bool HasPathValue => true;

    internal override string ValueToCommandLine()
    {
        return ArgumentValueSerializer.ShortenPath(Value, gameDir);
    }
}

/// <summary>
/// File location where RNG logging shall be stored to.
/// </summary>
/// <remarks>
/// The directory of the file must already exist.
/// </remarks>
public sealed class RandomLogFileArgument(IFileInfo value, IDirectoryInfo gameDir) : NamedArgument<IFileInfo>(GameArgumentNames.RandomLogFileArg, value, true)
{
    internal override bool HasPathValue => true;

    internal override string ValueToCommandLine()
    {
        return ArgumentValueSerializer.ShortenPath(Value, gameDir);
    }
}

/// <summary>
/// File location where logging shall be stored to.
/// </summary>
/// <remarks>
/// The directory of the file must already exist.
/// </remarks>
public sealed class LogFileArgument(IFileInfo value, IDirectoryInfo gameDir) : NamedArgument<IFileInfo>(GameArgumentNames.LogFileArg, value, true)
{
    internal override bool HasPathValue => true;

    internal override string ValueToCommandLine()
    {
        return ArgumentValueSerializer.ShortenPath(Value, gameDir);
    }
}

/// <summary>
/// UNKNOWN BEHAVIOR!
/// </summary>
internal sealed class ConsoleCommandFileArgument(IFileInfo value, IDirectoryInfo gameDir)
    : NamedArgument<IFileInfo>(GameArgumentNames.ConsoleCommandFileArg, value, true)
{
    internal override bool HasPathValue => true;

    internal override string ValueToCommandLine()
    {
        return ArgumentValueSerializer.ShortenPath(Value, gameDir);
    }
}

/// <summary>
/// Logging filter for events relevant for multiplayer synchronization. Default value is 15. 
/// </summary>
/// <remarks>This obviously is an enum, but it's not documented, so we leave it as a plain ushort value.</remarks>
public sealed class SyncLogFilterArgument(ushort value) : NamedArgument<ushort>(GameArgumentNames.SyncLogFilterArg, value, true);

/// <summary>
/// UNKNOWN BEHAVIOR! Not sure where it 'connects' to.
/// </summary>
internal sealed class ConnectPortArgument(uint value) : NamedArgument<uint>(GameArgumentNames.ConnectPortArg, value, true);

/// <summary>
/// UNKNOWN BEHAVIOR! Not sure where it 'connects' to.
/// </summary>
internal sealed class ConnectIPArgument(string value) : NamedArgument<string>(GameArgumentNames.ConnectIPArg, value, true);
#endregion