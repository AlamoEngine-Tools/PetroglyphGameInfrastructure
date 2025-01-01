using System.Collections.Generic;
using System.IO.Abstractions;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ArgumentCommandLineBuilderTest : GameArgumentTestBase
{
    private readonly IFileSystem _fs = new MockFileSystem();

    [Fact]
    public void TestEmptyList()
    {
        Assert.Equal(string.Empty, ArgumentCommandLineBuilder.BuildCommandLine(ArgumentCollection.Empty));
    }

    public static IEnumerable<object[]> GetBuildCommandLineTestData()
    {
        var fs = new MockFileSystem();
        var gameDir = fs.DirectoryInfo.New("game");

        var normalMod = new ModArgument(fs.DirectoryInfo.New("game/path"), gameDir, false);
        var steamMod = new ModArgument(fs.DirectoryInfo.New("123456"), gameDir, true);

        yield return [new GameArgument[] { }, string.Empty];
        yield return [new GameArgument[] { new TestFlagArg(false, true) }, string.Empty];
        yield return [new GameArgument[] { new WindowedArgument() }, "WINDOWED"];
        yield return [new GameArgument[] { new MCEArgument() }, "-MCE"];
        yield return [new GameArgument[] { new MapArgument("myMap") }, "MAP=myMap"];
        yield return [new GameArgument[] { new WindowedArgument(), new MapArgument("myMap") }, "WINDOWED MAP=myMap"];
        yield return [new GameArgument[] { new MapArgument("myMap"), new WindowedArgument() }, "MAP=myMap WINDOWED"];
        yield return [new GameArgument[] { new ModArgumentList([normalMod]) }, "MODPATH=PATH"];
        yield return [new GameArgument[] { new ModArgumentList([normalMod, steamMod]) }, "MODPATH=PATH STEAMMOD=123456"];
        yield return [new GameArgument[] { new ModArgumentList([steamMod, normalMod]) }, "STEAMMOD=123456 MODPATH=PATH"];
        yield return [new GameArgument[] { new ModArgumentList([normalMod, steamMod]), new WindowedArgument() }, "MODPATH=PATH STEAMMOD=123456 WINDOWED"];
        yield return [new GameArgument[] { new ModArgumentList([normalMod, steamMod]), new WindowedArgument(), new MCEArgument(), new MapArgument("myMap") },
            "MODPATH=PATH STEAMMOD=123456 WINDOWED -MCE MAP=myMap"];
        yield return [new GameArgument[] { new LanguageArgument(new LanguageInfo("de", LanguageSupportLevel.Default)) }, "LANGUAGE=GERMAN"];
        yield return [new GameArgument[] { new MonitorArgument(42) }, "MONITOR=42"];
        yield return [new GameArgument[] { new AILogStyleArgument(AILogStyle.Normal) }, "AILOGSTYLE=NORMAL"];
    }

    [Theory]
    [MemberData(nameof(GetBuildCommandLineTestData))]
    public void BuildCommandLine(IList<GameArgument> args, string expectedCommandLine)
    {
        var command = ArgumentCommandLineBuilder.BuildCommandLine(new ArgumentCollection(args));
        Assert.Equal(expectedCommandLine, command);
    }

    [Theory]
    [MemberData(nameof(GetInvalidArgumentListTestData))]
    public void TestInvalidList_Throws(IList<GameArgument> arguments, GameArgument failingArgument)
    {
        var collection = new ArgumentCollection(arguments);

        var e = Assert.Throws<GameArgumentException>(() => ArgumentCommandLineBuilder.BuildCommandLine(collection));
        Assert.Equal(failingArgument, e.Argument);
    }

    [Fact]
    public void TestDisabledDashedFlagArg()
    {
        var arg = new TestFlagArg(false, true);
        var command = ArgumentCommandLineBuilder.BuildCommandLine(new ArgumentCollection([arg]));
        Assert.Empty(command);
    }

    [Theory]
    [InlineData("path with space")]
    [InlineData("path&calc.exe")]
    public void TestModListHasInvalidArg_Throws(string invalidData)
    {
        var gameDir = _fs.DirectoryInfo.New("game");
        var modArg = new ModArgument(_fs.DirectoryInfo.New(invalidData), gameDir, false);

        var arg = new ModArgumentList([modArg]);
        Assert.Throws<GameArgumentException>(() => ArgumentCommandLineBuilder.BuildCommandLine(new ArgumentCollection([arg])));
    }

    [Fact]
    public void TestModListHasInvalidArg_SteamNotValidId_Throws()
    {
        var gameDir = _fs.DirectoryInfo.New("game");

        var modArg = new ModArgument(_fs.DirectoryInfo.New("notSteamId"), gameDir, true);
        var arg = new ModArgumentList([modArg]);

        Assert.Throws<GameArgumentException>(() => ArgumentCommandLineBuilder.BuildCommandLine(new ArgumentCollection([arg])));
    }
}