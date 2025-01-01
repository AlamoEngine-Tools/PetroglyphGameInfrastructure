using System;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ArgumentValidatorTest : GameArgumentTestBase
{
    [Theory]
    [MemberData(nameof(GetIllegalCharacterGameArgs))]
    public void CheckArgument_TestHasIllegalChar(GameArgument arg)
    {
        var reason = ArgumentValidator.CheckArgument(arg);
        Assert.Equal(ArgumentValidityStatus.IllegalCharacter, reason);
    }

    [Fact]
    public void CheckArgument_HasEmptyData()
    {
        var arg = TestNamedArg.FromValue(string.Empty);
        var reason = ArgumentValidator.CheckArgument(arg);
        Assert.Equal(ArgumentValidityStatus.EmptyData, reason);
    }

    [Fact]
    public void NameIsNotSupported_IsInvalid()
    {
        Assert.Equal(ArgumentValidityStatus.InvalidName, ArgumentValidator.CheckArgument(new TestNamedArg("NOTSUPPORTED")));
        Assert.Equal(ArgumentValidityStatus.InvalidName, ArgumentValidator.CheckArgument(new TestFlagArg("NOTSUPPORTED")));
    }

    [Theory]
    [MemberData(nameof(GetGameArgsWithSpaceValue))]
    public void CheckArgument_HasSpaces(GameArgument arg)
    {
        var reason = ArgumentValidator.CheckArgument(arg);
        Assert.Equal(ArgumentValidityStatus.PathContainsSpaces, reason);
    }

    [Fact]
    public void CheckArgument_ModListCorrectType()
    {
        Assert.Equal(ArgumentValidityStatus.InvalidData, ArgumentValidator.CheckArgument(new InvalidModListArg()));
        Assert.Equal(ArgumentValidityStatus.Valid, ArgumentValidator.CheckArgument(new ModArgumentList(Array.Empty<ModArgument>())));
    }

    [Theory]
    [MemberData(nameof(GetValidArguments))]
    public void IsValid(GameArgument arg)
    {
        Assert.True(arg.IsValid(out var reason));
        Assert.Equal(ArgumentValidityStatus.Valid, reason);
    }

    [Theory]
    [MemberData(nameof(GetWindowsAbsoluteTestPaths))]
    public void IsValid_PathBasedArgument(string value)
    {
        var fs = new MockFileSystem();
        var gameDir = fs.DirectoryInfo.New("game");
        var modDir = fs.DirectoryInfo.New(value);

        var arg = new ModArgument(modDir, gameDir, false);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.True(arg.IsValid(out var reason));
            Assert.Equal(ArgumentValidityStatus.Valid, reason);
        }
        else
        {
            Assert.False(arg.IsValid(out var reason));
            Assert.Equal(ArgumentValidityStatus.IllegalCharacter, reason);
        }
    }

    [Theory]
    [MemberData(nameof(GetWindowsAbsoluteTestPaths))]
    public void IsValid_NotPathBasedArgument(string value)
    {
        var arg = new CDKeyArgument(value);
        Assert.False(arg.IsValid(out var reason));
        Assert.Equal(ArgumentValidityStatus.IllegalCharacter, reason);
    }
}