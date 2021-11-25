using System.Collections.Generic;
using Castle.DynamicProxy.Generators;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test.Arguments;

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