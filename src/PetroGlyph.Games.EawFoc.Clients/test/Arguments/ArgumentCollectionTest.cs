using System.Collections;
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
        Assert.Equal(0, empty.Count);
    }

    [Fact]
    public void TestImmutable()
    {
        var args = new List<GameArgument> { new WindowedArgument() };
        var argList = new ArgumentCollection(args);
        
        args.Add(new MapArgument("Map"));

        Assert.Equal(1, argList.Count);
        var a = Assert.Single(argList);

        var e = ((IEnumerable)argList).GetEnumerator();
        Assert.True(e.MoveNext());
        var o = (GameArgument)e.Current;


        Assert.Equal(new WindowedArgument(), a);
        Assert.Equal(new WindowedArgument(), o);
    }
}