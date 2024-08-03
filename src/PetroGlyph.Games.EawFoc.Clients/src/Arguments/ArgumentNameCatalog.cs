using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Collections of supported Argument Names
/// </summary>
internal static class ArgumentNameCatalog
{
    internal const string ModListArg = "MODLIST";
    internal const string WindowedArg = "WINDOWED";
    internal const string MCEArg = "MCE";
    internal const string LowRamArg = "LOWRAM";
    internal const string SafeModeArg = "SAFEMODE";
    internal const string IgnoreAssertsArg = "IGNOREASSERTS";
    internal const string NoArtProcessArg = "NOARTPROCESS";
    internal const string RecordingArg = "RECORDING";
    internal const string MultiArg = "MULTI";
    internal const string PersistAssertsArg = "PERSISTASSERTS";
    internal const string NoTimeoutArg = "NOTIMEOUT";
    internal const string SuppressLogArg = "SUPRESSLOG"; // Typo as in binary!
    internal const string PreValidateAudioMessagesArg = "PREVALIDATE_AUDIO_MEGS";
    internal const string LeftLeftArg = "LEFTLEFT";
    internal const string DeepSyncArg = "DEEPSYNC";
    internal const string NoFowArg = "NOFOW";
    internal const string NoIntroArg = "NOINTRO";
    internal const string SaveLoadDebugArg = "SAVELOADDEBUG";
    internal const string RefCountTrackingArg = "REFCOUNTTRACKING";
    internal const string NoHardwareMouseArg = "NOHARDWAREMOUSE";
    internal const string ProcessEnglishAssetsAndExitArg = "PROCESS_ENGLISH_ASSETS_AND_EXIT";
    internal const string ProcessAssetsAndExitArg = "PROCESS_ASSETS_AND_EXIT";
    internal const string AttractArg = "ATTRACT";
    internal const string DebugUnitsArg = "DEBUG_UNITS";
    internal const string NoMenuArg = "NOMENU";
    internal const string FullScreenArg = "FULLSCREEN";

    internal const string LocalPortArg = "LOCALPORT";
    internal const string MonitorArg = "MONITOR";
    internal const string ScreenWidthArg = "SCREENWIDTH";
    internal const string ScreenHeightArg = "SCREENHEIGHT";
    internal const string FPSCapArg = "FPSCAP";
    internal const string FallbackPathArg = "FALLBACKPATH";
    internal const string ModPathArg = "MODPATH";
    internal const string SteamModArg = "STEAMMOD";
    internal const string LanguageArg = "LANGUAGE";
    internal const string OriginalAssetPathArg = "ORIGINAL_ASSET_PATH";
    internal const string RandomSeedArg = "RANDOMSEED";
    internal const string ExpCDKeyArg = "EXPCDKEY";
    internal const string CDKeyArg = "CDKEY";
    internal const string MPPlaybackFileArg = "MPPLAYBACKFILE";
    internal const string MPRecordFileArg = "MPRECORDFILE";
    internal const string MapArg = "MAP";
    internal const string SaveFolderArg = "SAVEFOLDER";
    internal const string QuickLoadRecordingArg = "QUICKLOADRECORDING";
    internal const string QuickLoadArg = "QUICKLOAD";
    internal const string ConfigArg = "CONFIG";
    internal const string ProfileArg = "PROFILE";
    internal const string BCast4Arg = "BCAST4";
    internal const string BCast3Arg = "BCAST3";
    internal const string BCast2Arg = "BCAST2";
    internal const string AILogStyleArg = "AILOGSTYLE";
    internal const string AILogFileArg = "AILOGFILE";
    internal const string SyncLogFilterArg = "SYNCLOGFILTER";
    internal const string RandomLogFileArg = "RANDOMLOGFILE";
    internal const string LogFileArg = "LOGFILE";
    internal const string ConsoleCommandFileArg = "CONSOLECOMMANDFILE";
    internal const string ConnectPortArg = "CONNECTPORT";
    internal const string ConnectIPArg = "CONNECTIP";

    /// <summary>
    /// Collection of all supported argument names.
    /// </summary>
    /// <remarks>The names are in upper case.</remarks>
    public static IReadOnlyCollection<string> AllSupportedArgumentNames =
        SupportedFlagArgumentNames.Union(SupportedKeyValueArgumentNames).Union(SyntheticArgumentNames).ToList();

    internal static IReadOnlyCollection<string> SyntheticArgumentNames => new[] { ModListArg };

    internal static IReadOnlyCollection<string> SupportedFlagArgumentNames => new HashSet<string>
    {
        WindowedArg,
        MCEArg,
        LowRamArg,
        SafeModeArg,
        IgnoreAssertsArg,
        NoArtProcessArg,
        RecordingArg,
        MultiArg,
        PersistAssertsArg,
        NoTimeoutArg,
        SuppressLogArg,
        PreValidateAudioMessagesArg,
        LeftLeftArg,
        DeepSyncArg,
        NoFowArg,
        NoIntroArg,
        SaveLoadDebugArg,
        RefCountTrackingArg,
        NoHardwareMouseArg,
        ProcessEnglishAssetsAndExitArg,
        ProcessAssetsAndExitArg,
        AttractArg,
        DebugUnitsArg,
        NoMenuArg,
        FullScreenArg
    };

    internal static IReadOnlyCollection<string> SupportedKeyValueArgumentNames => new HashSet<string>
    {
        LocalPortArg,
        MonitorArg,
        ScreenWidthArg,
        ScreenHeightArg,
        FPSCapArg,
        FallbackPathArg,
        ModPathArg,
        SteamModArg,
        LanguageArg,
        OriginalAssetPathArg,
        RandomSeedArg,
        ExpCDKeyArg,
        CDKeyArg,
        MPPlaybackFileArg,
        MPRecordFileArg,
        MapArg,
        RecordingArg,
        SaveFolderArg,
        QuickLoadRecordingArg,
        QuickLoadArg,
        ConfigArg,
        ProfileArg,
        BCast2Arg,
        BCast3Arg,
        BCast4Arg,
        AILogStyleArg,
        AILogFileArg,
        SyncLogFilterArg,
        RandomLogFileArg,
        LogFileArg,
        ConsoleCommandFileArg,
        ConnectPortArg,
        ConnectIPArg
    };
}