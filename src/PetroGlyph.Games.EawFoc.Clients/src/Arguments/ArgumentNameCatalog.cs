using System.Collections.Generic;
using System.Linq;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Collections of supported Argument Names
/// </summary>
public static class ArgumentNameCatalog
{
    public static IReadOnlyCollection<string> AllSupportedArgumentNames =
        SupportedFlagArgumentNames.Union(SupportedFlagArgumentNames).Union(SupportedKeyValueArgumentNames).ToList();

    public static IReadOnlyCollection<string> SyntheticArgumentNames => new[] { "MODLIST" };

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