﻿using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class UniqueArgumentCollectionBuilderTest
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

        var builder = new UniqueArgumentCollectionBuilder(args);
        Assert.Equal(2, builder.Build().Count);

        builder.Remove(new WindowedArgument());
        Assert.Single(builder.Build());

        builder.Add(new MonitorArgument(2));
        Assert.Equal(2u, builder.Build().First().Value);

        builder.Remove("MONITOR");
        Assert.Empty(builder.Build());
    }
}