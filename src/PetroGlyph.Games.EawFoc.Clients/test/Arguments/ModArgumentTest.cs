using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ModArgumentTest
{
    [Theory]
    [InlineData(true, "STEAMMOD")]
    [InlineData(false, "MODPATH")]
    public void Ctor_SetProps(bool isWorkshop, string expectedName)
    {
        var fs = new MockFileSystem();
        var arg = new ModArgument(fs.DirectoryInfo.New("123456"), fs.DirectoryInfo.New("."), isWorkshop);

        Assert.Equal(expectedName, arg.Name);
        Assert.Equal(!isWorkshop, arg.HasPathValue);
    }

    [Theory]
    [InlineData("path", false, true)]
    [InlineData("123", false, true)]
    [InlineData("123", true, true)]
    [InlineData("path", true, false)]
    public void IsValid_ConvertibleToSteam(string value, bool workshops, bool valid)
    {
        var fs = new MockFileSystem();
        var arg = new ModArgument(fs.DirectoryInfo.New(value), fs.DirectoryInfo.New("."), workshops);
        Assert.Equal(valid, arg.IsValid(out var reason));
        Assert.Equal(!valid ? ArgumentValidityStatus.InvalidData : ArgumentValidityStatus.Valid, reason);
    }
}