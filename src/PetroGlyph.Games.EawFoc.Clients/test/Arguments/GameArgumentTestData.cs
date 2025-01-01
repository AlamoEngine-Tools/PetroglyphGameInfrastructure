using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using PG.TestingUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class GameArgumentTestBase
{
    public const string ValidStringValue = "someValue";
    public const string InvalidStringValue = "some value";

    public static IEnumerable<string> GetValidStringValues =>
    [
        ValidStringValue,
        "some-value",
        "some/path",
        "some\\path",
        "../some\\./relative/path",
        "/path",
        "123456",
        "some_value"
    ];

    public static IEnumerable<object[]> GetWindowsAbsoluteTestPaths()
    {
        yield return ["C:/path"];
        yield return ["C:\\path"];
        yield return ["C:path"];
    }

    public static IEnumerable<object[]> GetInvalidArgumentListTestData()
    {
        yield return [new GameArgument[] { new WindowedArgument(), new MapArgument(InvalidStringValue) }, new MapArgument(InvalidStringValue)];
        yield return [new GameArgument[] { new MapArgument(InvalidStringValue), new WindowedArgument() }, new MapArgument(InvalidStringValue)];


        var fs = new MockFileSystem();
        fs.Initialize();

        var gameDir = fs.DirectoryInfo.New("game");

        var invalidModArg_PathWithSpace = new ModArgument(fs.DirectoryInfo.New(InvalidStringValue), gameDir, false);
        var invalidModArg_AbsolutePath = new ModArgument(fs.DirectoryInfo.New("x:/path"), gameDir, false);
        var invalidModArg_InvalidSteamId = new ModArgument(fs.DirectoryInfo.New(ValidStringValue), gameDir, true);

        yield return
        [
            new GameArgument[] { new WindowedArgument(), new ModArgumentList([invalidModArg_PathWithSpace]) },
            invalidModArg_PathWithSpace
        ];
        yield return
        [
            new GameArgument[] { new WindowedArgument(), new ModArgumentList([invalidModArg_InvalidSteamId]) },
            invalidModArg_InvalidSteamId
        ];

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield return
            [
                new GameArgument[] { new WindowedArgument(), new ModArgumentList([invalidModArg_AbsolutePath]) },
                invalidModArg_AbsolutePath
            ];
        }

        // Invalid Chars
        foreach (var invalidArg in GetIllegalCharacterGameArgs())
        {
            yield return [new[] {new WindowedArgument(), (GameArgument)invalidArg[0]}, invalidArg[0]];
        }

        // Has Spaces
        foreach (var invalidArg in GetGameArgsWithSpaceValue())
        {
            yield return [new[] { new WindowedArgument(), (GameArgument)invalidArg[0] }, invalidArg[0]];
        }
    }
    
    public static IEnumerable<object[]> GetIllegalCharacterGameArgs()
    {
        return GetIllegalCharacterValues().Select(value => (object[])[TestNamedArg.FromValue((string)value[0])]);
    }

    public static IEnumerable<object[]> GetIllegalCharacterValues()
    {
        for (var i = 0; i < 32; i++)
        {
            if (i is '\f' or '\n' or '\r' or '\t' or '\v')
                continue;
            yield return [$"abc{(char)i}"];
        }
        yield return ["abc?"];
        yield return ["abc*"];
        yield return ["abc:"];
        yield return ["abc|"];
        yield return ["abc>"];
        yield return ["abc<"];
        yield return ["abc&calc.exe"];
        yield return ["abc{'\"'}"];
        yield return ["x:/path:file.txt"];
    }

    public static IEnumerable<object[]> GetGameArgsWithSpaceValue()
    {
        yield return [TestNamedArg.FromValue("    ")];
        yield return [TestNamedArg.FromValue(" ")];
        yield return [TestNamedArg.FromValue("test\tvalue")];
        yield return [TestNamedArg.FromValue("test\fvalue")];
        yield return [TestNamedArg.FromValue("test\rvalue")];
        yield return [TestNamedArg.FromValue("test\nvalue")];
        yield return [TestNamedArg.FromValue("test\vvalue")];
        yield return [TestNamedArg.FromValue("test value")];
        yield return [TestNamedArg.FromValue("test\\path with space\\")];
        yield return [TestNamedArg.FromValue("testvalue ")];
    }

    public static IEnumerable<object[]> GetValidArguments()
    {
        var fs = new MockFileSystem();
        fs.Initialize();

        var gameDir = fs.DirectoryInfo.New("game");
        var modDir = fs.DirectoryInfo.New("game/mods/myMod");
        var steamModDir = fs.DirectoryInfo.New("game/mods/123456");

        var gameFile = fs.FileInfo.New("game/file.txt");

        foreach (var value in GetValidStringValues)
            yield return [TestNamedArg.FromValue(value)];

        yield return [new TestFlagArg(true, true)];
        yield return [new TestFlagArg(false, true)];
        yield return [new TestFlagArg(true, false)];
        yield return [new TestFlagArg(false, false)];

        // Flags
        yield return [new WindowedArgument()];
        yield return [new SafeMode()];
        yield return [new LowRamArgument()];
        yield return [new MCEArgument()];
        // Debug Flags
        yield return [new IgnoreAssertsArgument()];
        yield return [new NoArtProcessArgument()];
        yield return [new RecordingArgument()];
        yield return [new MultiArgument()];
        yield return [new PersistAssertsArgument()];
        yield return [new NoTimeoutArgument()];
        yield return [new SuppressLogArgument()];
        yield return [new PreValidateAudioMessagesArgument()];
        yield return [new LeftLeftArgument()];
        yield return [new DeppSyncArgument()];
        yield return [new NoFowArgument()];
        yield return [new NoIntroArgument()];
        yield return [new SaveLoadDebugArgument()];
        yield return [new RefCountTrackingArgument()];
        yield return [new NoHardwareMouseArgument()];
        yield return [new ProcessEnglishAssetsAndExitArgument()];
        yield return [new ProcessAssetsAndExitArgument()];
        yield return [new AttractArgument()];
        yield return [new DebugUnitsArgument()];
        yield return [new NoMenuArgument()];
        yield return [new FullScreenArgument()];
        // Key/Value
        yield return [new LanguageArgument(new LanguageInfo("en", LanguageSupportLevel.Default))];
        yield return [new LanguageArgument(new LanguageInfo("de", LanguageSupportLevel.SFX))];
        yield return [new LocalPortArgument(TestHelpers.RandomUInt())];
        yield return [new MonitorArgument(TestHelpers.RandomUInt())];
        yield return [new ScreenWidthArgument(TestHelpers.RandomUInt())];
        yield return [new ScreenHeightArgument(TestHelpers.RandomUInt())];
        yield return [new FPSCapArgument(TestHelpers.RandomUInt())];
        // Debug 
        yield return [new OriginalAssetPathArgument(modDir, gameDir)];
        yield return [new RandomSeedArgument(TestHelpers.RandomUInt())];
        yield return [new ExpCDKeyArgument(TestHelpers.GetRandom(GetValidStringValues))];
        yield return [new CDKeyArgument(TestHelpers.GetRandom(GetValidStringValues))];
        yield return [new MpPlaybackFileArgument(gameFile, gameDir)];
        yield return [new MapArgument(TestHelpers.GetRandom(GetValidStringValues))];
        yield return [new SaveFolderArgument(modDir, gameDir)];
        yield return [new QuickLoadRecordingArgument(TestHelpers.GetRandom(GetValidStringValues))];
        yield return [new QuickLoadArgument(TestHelpers.GetRandom(GetValidStringValues))];
        yield return [new ConfigArgument(TestHelpers.GetRandom(GetValidStringValues))];
        yield return [new ProfileArgument(TestHelpers.RandomUInt())];
        yield return [new BCast2Argument(TestHelpers.RandomUInt())];
        yield return [new BCast3Argument(TestHelpers.RandomUInt())];
        yield return [new BCast4Argument(TestHelpers.RandomUInt())];
        yield return [new AILogStyleArgument(TestHelpers.GetRandomEnum<AILogStyle>())];
        yield return [new AILogFileArgument(gameFile, gameDir)];
        yield return [new RandomLogFileArgument(gameFile, gameDir)];
        yield return [new LogFileArgument(gameFile, gameDir)];
        yield return [new ConsoleCommandFileArgument(gameFile, gameDir)];
        yield return [new SyncLogFilterArgument(TestHelpers.RandomUShort())];
        yield return [new ConnectPortArgument(TestHelpers.RandomUInt())];
        yield return [new ConnectIPArgument(TestHelpers.GetRandom(GetValidStringValues))];
        // Mod
        yield return [new ModArgumentList([])];
        yield return [new ModArgumentList([new ModArgument(modDir, gameDir, false)])];
        yield return [new ModArgumentList([new ModArgument(steamModDir, gameDir, true)])];
        yield return
        [
            new ModArgumentList([
                new ModArgument(fs.DirectoryInfo.New(TestHelpers.GetRandom(GetValidStringValues)), gameDir, false),
                new ModArgument(fs.DirectoryInfo.New(TestHelpers.GetRandom(GetValidStringValues)), gameDir, false),
            ])
        ];
    }
}

public class TestNamedArg(string name, string value, bool isDebug) : NamedArgument<string>(name, value, isDebug)
{
    public TestNamedArg(string name) : this(name, GameArgumentTestBase.ValidStringValue, false)
    {
    }

    public static TestNamedArg FromValue(string value)
    {
        var name = TestHelpers.GetRandom(GameArgumentNames.SupportedKeyValueArgumentNames);
        return new TestNamedArg(name, value, false);
    }
}

public class TestFlagArg(string name, bool value, bool dashed = false, bool debug = false)
    : FlagArgument(name, value, dashed, debug)
{
    public TestFlagArg(bool value, bool dashed) : this(
        TestHelpers.GetRandom(GameArgumentNames.SupportedFlagArgumentNames), value, dashed)
    {
    }

    public TestFlagArg(string name) : this(name, true)
    {
    }
}

public class LowerCaseNameArg()
    : FlagArgument(TestHelpers.GetRandom(GameArgumentNames.AllInternalSupportedArgumentNames).ToLowerInvariant(), true);

public class InvalidModListArg : GameArgument
{
    public override ArgumentKind Kind => ArgumentKind.ModList;

    internal InvalidModListArg() : base(GameArgumentNames.ModListArg, GameArgumentTestBase.ValidStringValue)
    {
    }
}


[SerializeEnumValue]
public enum MyEnumSerializeByValue
{
    A = 1,
    B = 2
}

public enum SomeRandomEnum
{
    Value
}