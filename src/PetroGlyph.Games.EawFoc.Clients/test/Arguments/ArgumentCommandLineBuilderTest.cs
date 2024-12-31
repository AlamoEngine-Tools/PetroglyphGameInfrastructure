using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ArgumentCommandLineBuilderTest : GameArgumentTestBase
{
    [Fact]
    public void TestEmptyList()
    {
        Assert.Equal(string.Empty, ArgumentCommandLineBuilder.BuildCommandLine(ArgumentCollection.Empty));
    }

    public static IEnumerable<object[]> GetBuildCommandLineTestData()
    {
        var normalMod = new ModArgument("path", false);
        var steamMod = new ModArgument("123456", true);

        yield return [new GameArgument[] { }, string.Empty];
        yield return [new GameArgument[] { new TestFlagArg(false, true) }, string.Empty];
        yield return [new GameArgument[] { new WindowedArgument() }, "WINDOWED"];
        yield return [new GameArgument[] { new MCEArgument() }, "-MCE"];
        yield return [new GameArgument[] { new MapArgument("myMap") }, "MAP=myMap"];
        yield return [new GameArgument[] { new WindowedArgument(), new MapArgument("myMap") }, "WINDOWED MAP=myMap"];
        yield return [new GameArgument[] { new MapArgument("myMap"), new WindowedArgument() }, "MAP=myMap WINDOWED"];
        yield return [new GameArgument[] { new ModArgumentList([normalMod]) }, "MODPATH=path"];
        yield return [new GameArgument[] { new ModArgumentList([normalMod, steamMod]) }, "MODPATH=path STEAMMOD=123456"];
        yield return [new GameArgument[] { new ModArgumentList([steamMod, normalMod]) }, "STEAMMOD=123456 MODPATH=path"];
        yield return [new GameArgument[] { new ModArgumentList([normalMod, steamMod]), new WindowedArgument() }, "MODPATH=path STEAMMOD=123456 WINDOWED"];
        yield return [new GameArgument[] { new ModArgumentList([normalMod, steamMod]), new WindowedArgument(), new MCEArgument(), new MapArgument("myMap") }, 
            "MODPATH=path STEAMMOD=123456 WINDOWED -MCE MAP=myMap"];
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

    [Fact]
    public void TestInvalidModList_Throws()
    {
        var arg = new InvalidModListArg();
        Assert.Throws<GameArgumentException>(() => ArgumentCommandLineBuilder.BuildCommandLine(new ArgumentCollection([arg])));
    }

    [Theory]
    [MemberData(nameof(GetIllegalCharacterValues), MemberType = typeof(ArgumentValidatorTest))]
    [InlineData("path with space")]
    [InlineData("path&calc.exe")]
    public void TestModListHasInvalidArg_Throws(string invalidData)
    {
        var modArg = new ModArgument(invalidData, false);
        var arg = new ModArgumentList([modArg]);

        Assert.Throws<GameArgumentException>(() => ArgumentCommandLineBuilder.BuildCommandLine(new ArgumentCollection([arg])));
    }

    [Fact]
    public void TestModListHasInvalidArg_SteamNotValidId_Throws()
    {
        var modArg = new ModArgument("notSteamId", true);
        var arg = new ModArgumentList([modArg]);

        Assert.Throws<GameArgumentException>(() => ArgumentCommandLineBuilder.BuildCommandLine(new ArgumentCollection([arg])));
    }
}