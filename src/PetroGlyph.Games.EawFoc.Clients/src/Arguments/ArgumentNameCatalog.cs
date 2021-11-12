using System.Collections.Generic;
using System.Linq;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Collections of supported Argument Names
/// </summary>
public static class ArgumentNameCatalog
{
    public const string ModListArg = "MODLIST";
    public const string WindowedArg = "WINDOWED";
    public const string MCEArg = "MCE";
    public const string SafeModeArg = "SAFEMODE";
    public const string IgnoreAssertsArg = "IGNOREASSERTS";
    public const string NoArtProcessArg = "NOARTPROCESS";
    public const string RecordingArg = "RECORDING";
    public const string MultiArg = "MULTI";
    public const string PersistAssertsArg = "PERSISTASSERTS";
    public const string NoTimeoutArg = "NOTIMEOUT";
    public const string SuppressLogArg = "SUPRESSLOG"; // Typo as in binary!
    public const string PreValidateAudioMessagesArg = "PREVALIDATE_AUDIO_MEGS";
    public const string LeftLeftArg = "LEFTLEFT";
    public const string DeepSyncArg = "DEEPSYNC";
    public const string NoFowArg = "NOFOW";
    public const string NoIntroArg = "NOINTRO";
    public const string SaveLoadDebugArg = "SAVELOADDEBUG";
    public const string RefCountTrackingArg = "REFCOUNTTRACKING";
    public const string NoHardwareMouseArg = "NOHARDWAREMOUSE";
    public const string ProcessEnglishAssetsAndExitArg = "PROCESS_ENGLISH_ASSETS_AND_EXIT";
    public const string ProcessAssetsAndExitArg = "PROCESS_ASSETS_AND_EXIT";
    public const string AttractArg = "ATTRACT";
    public const string DebugUnitsArg = "DEBUG_UNITS";
    public const string NoMenuArg = "NOMENU";
    public const string FullScreenArg = "FULLSCREEN";

    public const string LocalPortArg = "LOCALPORT";
    public const string MonitorArg = "MONITOR";
    public const string ScreenWidthArg = "SCREENWIDTH";
    public const string ScreenHeightArg = "SCREENHEIGHT";
    public const string FPSCapArg = "FPSCAP";
    public const string FallbackPathArg = "FALLBACKPATH";
    public const string ModPathArg = "MODPATH";
    public const string SteamModArg = "STEAMMOD";
    public const string LanguageArg = "LANGUAGE";
    public const string OriginalAssetPathArg = "ORIGINAL_ASSET_PATH";
    public const string RandomSeedArg = "RANDOMSEED";
    public const string ExpCDKeyArg = "EXPCDKEY";
    public const string CDKeyArg = "CDKEY";
    public const string MPPlaybackFileArg = "MPPLAYBACKFILE";
    public const string MPRecordFileArg = "MPRECORDFILE";
    public const string MapArg = "MAP";
    public const string SaveFolderArg = "SAVEFOLDER";
    public const string QuickLoadRecordingArg = "QUICKLOADRECORDING";
    public const string QuickLoadArg = "QUICKLOAD";
    public const string ConfigArg = "CONFIG";
    public const string ProfileArg = "PROFILE";
    public const string BCast4Arg = "BCAST4";
    public const string BCast3Arg = "BCAST3";
    public const string BCast1Arg = "BCAST2";
    public const string AILogSytyleArg = "AILOGSTYLE";
    public const string AILogFileArg = "AILOGFILE";
    public const string SyncLogFilterArg = "SYNCLOGFILTER";
    public const string RandomLogFileArg = "RANDOMLOGFILE";
    public const string LogFileArg = "LOGFILE";
    public const string ConsoleCommandFileArg = "CONSOLECOMMANDFILE";
    public const string ConnectPortArg = "CONNECTPORT";
    public const string ConnectIPArg = "CONNECTIP";

    public static IReadOnlyCollection<string> AllSupportedArgumentNames =
        SupportedFlagArgumentNames.Union(SupportedFlagArgumentNames).Union(SupportedKeyValueArgumentNames).ToList();

    public static IReadOnlyCollection<string> SyntheticArgumentNames => new[] { ModListArg };

    public static IReadOnlyCollection<string> SupportedFlagArgumentNames => new HashSet<string>
    {
        "WINDOWED",
        "MCE",
        "LOWRAM",
        "SAFEMODE",
        "IGNOREASSERTS",
        "NOARTPROCESS",
        "RECORDING",
        "MULTI",
        "PERSISTASSERTS",
        "NOTIMEOUT",
        "SUPRESSLOG",
        "PREVALIDATE_AUDIO_MEGS",
        "LEFTLEFT",
        "DEEPSYNC",
        "NOFOW",
        "NOINTRO",
        "SAVELOADDEBUG",
        "REFCOUNTTRACKING",
        "NOHARDWAREMOUSE",
        "PROCESS_ENGLISH_ASSETS_AND_EXIT",
        "PROCESS_ASSETS_AND_EXIT",
        "ATTRACT",
        "DEBUG_UNITS",
        "NOMENU",
        "FULLSCREEN"

    };

    public static IReadOnlyCollection<string> SupportedKeyValueArgumentNames => new HashSet<string>
    {
        "LOCALPORT",
        "MONITOR",
        "SCREENWIDTH",
        "SCREENHEIGHT",
        "FPSCAP",
        "FALLBACKPATH",
        "MODPATH",
        "STEAMMOD",
        "LANGUAGE",
        "ORIGINAL_ASSET_PATH",
        "RANDOMSEED",
        "EXPCDKEY",
        "CDKEY",
        "MPPLAYBACKFILE",
        "MPRECORDFILE",
        "MAP",
        "RECORDING",
        "SAVEFOLDER",
        "QUICKLOADRECORDING",
        "QUICKLOAD",
        "CONFIG",
        "PROFILE",
        "BCAST4",
        "BCAST3",
        "BCAST2",
        "AILOGSTYLE",
        "AILOGFILE",
        "SYNCLOGFILTER",
        "RANDOMLOGFILE",
        "LOGFILE",
        "CONSOLECOMMANDFILE",
        "CONNECTPORT",
        "CONNECTIP"
    };
}