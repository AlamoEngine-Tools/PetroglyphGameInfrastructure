﻿using System;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ModArgumentListTest
{
    [Fact]
    public void TestProps()
    {
        var arg = new ModArgumentList(new List<ModArgument> { new ModArgument("path", false) });
        Assert.Equal(ArgumentNameCatalog.ModListArg, arg.Name);
        Assert.Equal(ArgumentKind.ModList, arg.Kind);
        Assert.Empty(arg.ValueToCommandLine());
        Assert.Single(arg.Value);
    }

    [Fact]
    public void TestEmpty()
    {
        var arg = ModArgumentList.Empty;
        Assert.Empty(arg.Value);
        Assert.True(arg.IsValid(out _));
    }

    [Fact]
    public void TestEquals()
    {
        var a = ModArgumentList.Empty;
        var b = new ModArgumentList(new List<ModArgument>());
        Assert.True(a.Equals(b));

        var c = new ModArgumentList(new List<ModArgument> { new ModArgument("path", false) });
        Assert.False(a.Equals(c));
        var d = new ModArgumentList(new List<ModArgument> { new ModArgument("path", true) });
        Assert.False(c.Equals(d));
        var e = new ModArgumentList(new List<ModArgument> { new ModArgument("path", true) });
        Assert.True(d.Equals(e));
    }

    private class InvalidModArg : IGameArgument<string>
    {
        public bool Equals(IGameArgument? other)
        {
            throw new NotImplementedException();
        }

        public ArgumentKind Kind => ArgumentKind.Flag;
        public bool DebugArgument => false;
        public string Name => null!;
        public string Value => null!;

        object IGameArgument.Value => Value;

        public string ValueToCommandLine()
        {
            throw new NotImplementedException();
        }

        public bool IsValid(out ArgumentValidityStatus reason)
        {
            throw new NotImplementedException();
        }
    }
}