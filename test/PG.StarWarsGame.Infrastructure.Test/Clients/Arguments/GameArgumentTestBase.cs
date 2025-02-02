using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Test.Clients.Arguments;

public class GameArgumentTestBase
{
    public const string ValidStringValue = "someValue";

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

    public static IEnumerable<string> GetValuesWithSpaces => GetPathsWithSpaces.Union([
        "     ",
        " ",
        "testvalue ",
        " testvalue",
        "test\tvalue",
        "test\fvalue",
        "test\rvalue",
        "test\nvalue",
        "test\vvalue",
    ]);

    public static IEnumerable<string> GetPathsWithSpaces => [
        "test value",
        "test/path with space/",
        "with space/test"
    ];

    public static IEnumerable<string> GetIllegalCharacterValues()
    {
        for (var i = 0; i < 32; i++)
        {
            if (i is '\f' or '\n' or '\r' or '\t' or '\v')
                continue;
            yield return $"abc{(char)i}";
        }
        yield return "abc?";
        yield return "abc*";
        yield return "abc:";
        yield return "abc|";
        yield return "abc>";
        yield return "abc<";
        yield return "abc&calc.exe";
        yield return "abc{'\"'}";
        yield return "x:/path:file.txt";
    }

    public static IEnumerable<string> GetWindowsAbsoluteTestPaths =>
    [
        "C:/path",
        "C:\\path",
        "C:path"
    ];

    private static IEnumerable<GameArgument> GetNonPathKeyValueArgs(string value)
    {
        yield return new ExpCDKeyArgument(value);
        yield return new CDKeyArgument(value);
        yield return new MapArgument(value);
        yield return new QuickLoadRecordingArgument(value);
        yield return new QuickLoadArgument(value);
        yield return new ConfigArgument(value);
        yield return new ConnectIPArgument(value);
    }
    
    public static IEnumerable<GameArgument> GetPathKeyValueArgs(string path, string gamePath, IFileSystem fs)
    {
        yield return new ModArgument(fs.DirectoryInfo.New(path), fs.DirectoryInfo.New(gamePath), false);
        yield return new OriginalAssetPathArgument(fs.DirectoryInfo.New(path), fs.DirectoryInfo.New(gamePath));
        yield return new MpPlaybackFileArgument(fs.FileInfo.New(path), fs.DirectoryInfo.New(gamePath));
        yield return new SaveFolderArgument(fs.DirectoryInfo.New(path), fs.DirectoryInfo.New(gamePath));
        yield return new AILogFileArgument(fs.FileInfo.New(path), fs.DirectoryInfo.New(gamePath));
        yield return new RandomLogFileArgument(fs.FileInfo.New(path), fs.DirectoryInfo.New(gamePath));
        yield return new LogFileArgument(fs.FileInfo.New(path), fs.DirectoryInfo.New(gamePath));
        yield return new ConsoleCommandFileArgument(fs.FileInfo.New(path), fs.DirectoryInfo.New(gamePath));
    }


    public static IEnumerable<object[]> GetInvalidArguments()
    {
        var fs = new MockFileSystem();
        fs.Initialize();
        
        foreach (var argsWithEmptyString in GetNonPathKeyValueArgs(string.Empty))
            yield return [argsWithEmptyString, ArgumentValidityStatus.EmptyData];


        yield return [new TestNamedArg("NOTSUPPORTED"), ArgumentValidityStatus.InvalidName];
        yield return [new TestFlagArg("NOTSUPPORTED"), ArgumentValidityStatus.InvalidName];

        foreach (var illegalCharacterValue in GetIllegalCharacterValues())
        {
            yield return [TestNamedArg.FromValue(illegalCharacterValue), ArgumentValidityStatus.IllegalCharacter];
        }

        foreach (var valuesWithSpace in GetValuesWithSpaces)
        {
            foreach (var argWithSpace in GetNonPathKeyValueArgs(valuesWithSpace))
                yield return [argWithSpace, ArgumentValidityStatus.PathContainsSpaces];
        }

        foreach (var pathWithSpace in GetPathsWithSpaces)
        {
            foreach (var argWithSpace in GetPathKeyValueArgs(pathWithSpace, ".", fs))
                yield return [argWithSpace, ArgumentValidityStatus.PathContainsSpaces];
        }

        foreach (var windowsAbsolutePath in GetWindowsAbsoluteTestPaths)
        {
            // These args do not expect paths, thus absolute paths using 'X:' pattern are not allowed
            foreach (var argWithAbsolutePath in GetNonPathKeyValueArgs(windowsAbsolutePath))
                yield return [argWithAbsolutePath, ArgumentValidityStatus.IllegalCharacter];

            // Absolute paths using 'X:' pattern are only allowed on Windows systems
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (var argWithAbsolutePath in GetPathKeyValueArgs(windowsAbsolutePath, ".", fs))
                    yield return [argWithAbsolutePath, ArgumentValidityStatus.IllegalCharacter];
            }
        }
    }
    public static IEnumerable<object[]> GetInvalidArgumentListTestData()
    {
        foreach (var invalidArgumentData in GetInvalidArguments())
        {
            var invalidArgument = (GameArgument) invalidArgumentData[0];
            yield return [new[] { new WindowedArgument(), invalidArgument }, invalidArgument];
            yield return [new[] { invalidArgument, new WindowedArgument() }, invalidArgument];
        }
    }

    public static IEnumerable<object[]> GetValidArguments()
    {
        var fs = new MockFileSystem();
        fs.Initialize();

        var gameDir = fs.DirectoryInfo.New("with space/game");
        var modDir = fs.DirectoryInfo.New("with space/game/mods/myMod");
        var externalMod = fs.DirectoryInfo.New("myMod");
        var relativeMod = fs.DirectoryInfo.New("with space/otherGame/mods/myMod");
        var steamModDir = fs.DirectoryInfo.New("with space/game/mods/123456");

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

        yield return [new LanguageArgument(new LanguageInfo("en", LanguageSupportLevel.Default))];
        yield return [new LanguageArgument(new LanguageInfo("de", LanguageSupportLevel.SFX))];
        yield return [new LocalPortArgument(TestHelpers.RandomUInt())];
        yield return [new MonitorArgument(TestHelpers.RandomUInt())];
        yield return [new ScreenWidthArgument(TestHelpers.RandomUInt())];
        yield return [new ScreenHeightArgument(TestHelpers.RandomUInt())];
        yield return [new FPSCapArgument(TestHelpers.RandomUInt())];
        yield return [new RandomSeedArgument(TestHelpers.RandomUInt())];
        yield return [new ProfileArgument(TestHelpers.RandomUInt())];
        yield return [new BCast2Argument(TestHelpers.RandomUInt())];
        yield return [new BCast3Argument(TestHelpers.RandomUInt())];
        yield return [new BCast4Argument(TestHelpers.RandomUInt())];
        yield return [new AILogStyleArgument(TestHelpers.GetRandomEnum<AILogStyle>())];
        yield return [new SyncLogFilterArgument(TestHelpers.RandomUShort())];
        yield return [new ConnectPortArgument(TestHelpers.RandomUInt())];
        foreach (var nonPathArg in GetNonPathKeyValueArgs(TestHelpers.GetRandom(GetValidStringValues)))
            yield return [nonPathArg];
        foreach (var pathArg in GetPathKeyValueArgs(TestHelpers.GetRandom(GetValidStringValues), gameDir.FullName, fs))
            yield return [pathArg];

        // Mod
        yield return [new ModArgumentList([])];
        yield return [new ModArgumentList([new ModArgument(modDir, gameDir, false)])];
        yield return [new ModArgumentList([new ModArgument(externalMod, gameDir, false)])];
        yield return [new ModArgumentList([new ModArgument(relativeMod, gameDir, false)])];
        yield return [new ModArgumentList([new ModArgument(steamModDir, gameDir, true)])];
        yield return
        [
            new ModArgumentList([
                new ModArgument(fs.DirectoryInfo.New(TestHelpers.GetRandom(GetValidStringValues)), gameDir, false),
                new ModArgument(fs.DirectoryInfo.New(TestHelpers.GetRandom(GetValidStringValues)), gameDir, false),
            ])
        ];

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (var absolutePath in GetWindowsAbsoluteTestPaths)
            foreach (var absolutePathArg in GetPathKeyValueArgs(absolutePath, ".", fs))
                yield return [absolutePathArg];
        }
    }
}