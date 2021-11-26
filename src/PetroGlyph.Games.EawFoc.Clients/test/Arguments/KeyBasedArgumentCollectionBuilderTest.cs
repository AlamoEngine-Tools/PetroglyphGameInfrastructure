using System.Collections.Generic;
using System.Linq;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test.Arguments;

public class KeyBasedArgumentCollectionBuilderTest
{
    [Fact]
    public void TestBuilder()
    {
        var args = new ArgumentCollection(new List<IGameArgument>
        {
            new WindowedArgument(),
            new WindowedArgument(),
            new MonitorArgument(1)
        });

        var builder = new KeyBasedArgumentCollectionBuilder(args);
        Assert.Equal(2, builder.Build().Count);

        builder.Remove(new WindowedArgument());
        Assert.Single(builder.Build());

        builder.Add(new MonitorArgument(2));
        Assert.Equal(2u, builder.Build().First().Value);

        builder.Remove("MONITOR");
        Assert.Empty(builder.Build());
    }
}