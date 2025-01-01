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
    [MemberData(nameof(GetValidArguments))]
    public void IsValid_ArgumentValid(GameArgument arg)
    {
        Assert.Equal(ArgumentValidityStatus.Valid, ArgumentValidator.Validate(arg));
    }
    
    [Theory]
    [MemberData(nameof(GetInvalidArguments))]
    public void IsValid_ArgumentNotValid(GameArgument arg, ArgumentValidityStatus expectedReason)
    {
        Assert.Equal(expectedReason, ArgumentValidator.Validate(arg));
    }

    //[Theory]
    //[MemberData(nameof(GetWindowsAbsoluteTestPaths))]
    //public void IsValid_PathBasedArgument(string value)
    //{
    //    var fs = new MockFileSystem();
    //    var gameDir = fs.DirectoryInfo.New("game");
    //    var modDir = fs.DirectoryInfo.New(value);

    //    var arg = new ModArgument(modDir, gameDir, false);

    //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //    {
    //        Assert.True(arg.IsValid(out var reason));
    //        Assert.Equal(ArgumentValidityStatus.Valid, reason);
    //    }
    //    else
    //    {
    //        Assert.False(arg.IsValid(out var reason));
    //        Assert.Equal(ArgumentValidityStatus.IllegalCharacter, reason);
    //    }
    //}

    //[Theory]
    //[MemberData(nameof(GetWindowsAbsoluteTestPaths))]
    //public void IsValid_NotPathBasedArgument(string value)
    //{
    //    var arg = new CDKeyArgument(value);
    //    Assert.False(arg.IsValid(out var reason));
    //    Assert.Equal(ArgumentValidityStatus.IllegalCharacter, reason);
    //}
}