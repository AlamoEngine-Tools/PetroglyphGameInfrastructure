using System.Collections;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients.Arguments;

public class ArgumentCollectionTest
{
    [Fact]
    public void TestEmpty()
    {
        var empty = ArgumentCollection.Empty;
        Assert.Empty(empty);
#pragma warning disable xUnit2013
        Assert.Equal(0, empty.Count);
#pragma warning restore xUnit2013
    }

    [Fact]
    public void TestImmutable()
    {
        var args = new List<GameArgument> { new WindowedArgument() };
        var argList = new ArgumentCollection(args);
        
        args.Add(new MapArgument("Map"));

#pragma warning disable xUnit2013
        Assert.Equal(1, argList.Count);
#pragma warning restore xUnit2013
        var a = Assert.Single(argList);

        var e = ((IEnumerable)argList).GetEnumerator();
        Assert.True(e.MoveNext());
        var o = (GameArgument)e.Current!;


        Assert.Equal(new WindowedArgument(), a);
        Assert.Equal(new WindowedArgument(), o);
    }
}