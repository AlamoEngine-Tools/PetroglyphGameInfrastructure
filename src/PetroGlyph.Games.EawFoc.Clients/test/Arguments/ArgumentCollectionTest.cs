using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ArgumentCollectionTest
{
    [Fact]
    public void TestEmpty()
    {
        var empty = ArgumentCollection.Empty;
        Assert.Empty(empty);
    }

    [Fact]
    public void TestImmutable()
    {
        var args = new List<IGameArgument> { new WindowedArgument() };
        var argList = new ArgumentCollection(args);
        args.Add(new MapArgument("Map"));
        Assert.Single(argList);
    }
}