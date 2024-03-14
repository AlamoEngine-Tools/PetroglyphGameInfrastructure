using PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ModArgumentTest
{
    [Theory]
    [InlineData("path", false, true)]
    [InlineData("123", false, true)]
    [InlineData("123", true, true)]
    [InlineData("path", true, false)]
    public void TestValid(string value, bool workshops, bool valid)
    {
        var arg = new ModArgument(value, workshops);
        Assert.Equal(valid, arg.IsValid(out _));
    }
}