using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test.Arguments;

public class ArgumentCommandLineBuilderTest
{
    private readonly ArgumentCommandLineBuilder _service;
    private readonly Mock<IArgumentValidator> _validator;

    public ArgumentCommandLineBuilderTest()
    {
        var sc = new ServiceCollection();
        _validator = new Mock<IArgumentValidator>();
        sc.AddTransient(_ => _validator.Object);
        _service = new ArgumentCommandLineBuilder(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestEmptyList()
    {
        Assert.Equal(string.Empty, _service.BuildCommandLine(ArgumentCollection.Empty));
    }

    [Fact]
    public void TestInvalidList_Throws()
    {
        _validator.SetupSequence(v =>
                v.CheckArgument(It.IsAny<IGameArgument>(), out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
            .Returns(ArgumentValidityStatus.Valid)
            .Returns(ArgumentValidityStatus.InvalidData);
        var collection = new ArgumentCollection(new List<IGameArgument>
        {
            new WindowedArgument(),
            new MapArgument("test")
        });
        var e = Assert.Throws<GameArgumentException>(() => _service.BuildCommandLine(collection));
        Assert.Equal(new MapArgument("test"), e.Argument);
    }

    [Fact]
    public void TestMultipleArgs()
    {
        var mapName = "MAP";
        var mapValue = "test";
        var windowed = "WINDOWED";
        _validator.Setup(v =>
            v.CheckArgument(It.IsAny<FlagArgument>(), out windowed, out It.Ref<string>.IsAny));
        _validator.Setup(v =>
            v.CheckArgument(It.IsAny<NamedArgument<string>>(), out mapName, out mapValue));

        var collection = new ArgumentCollection(new List<IGameArgument>
        {
            new WindowedArgument(),
            new MapArgument("test")
        });
        Assert.Equal("WINDOWED MAP=test", _service.BuildCommandLine(collection));
    }

    [Fact]
    public void TestFlagArg()
    {
        var arg = new WindowedArgument();
        var command = _service.ToCommandLine(arg, "WINDOWED", "True");
        Assert.Equal("WINDOWED", command);
    }

    [Fact]
    public void TestDisabledFlagArg()
    {
        var arg = new DisabledFlag(false);
        var command = _service.ToCommandLine(arg, "FLAG", "False");
        Assert.Empty(command);
    }

    [Fact]
    public void TestDashedFlagArg()
    {
        var arg = new MCEArgument();
        var command = _service.ToCommandLine(arg, "MCE", "True");
        Assert.Equal("-MCE", command);
    }

    [Fact]
    public void TestDisabledDashedFlagArg()
    {
        var arg = new DisabledFlag(true);
        var command = _service.ToCommandLine(arg, "FLAG", "False");
        Assert.Empty(command);
    }

    [Fact]
    public void TestKeyValueArg()
    {
        var arg = new MapArgument("myMap");
        var command = _service.ToCommandLine(arg, "MAP", "myMap");
        Assert.Equal("MAP=myMap", command);
    }

    [Fact]
    public void TestInvalidModList_Throws()
    {
        var arg = new InvalidModListArg();
        Assert.Throws<GameArgumentException>(() => _service.ToCommandLine(arg, "MODLIST", string.Empty));
    }

    [Fact]
    public void TestModListEmpty()
    {
        var arg = new ModArgumentList(Array.Empty<IGameArgument<string>>());
        var command = _service.ToCommandLine(arg, "MODLIST", string.Empty);
        Assert.Empty(command);
    }

    [Fact]
    public void TestModListHasInvalidArg_Throws()
    {
        var modArg = new ModArgument("path", false);
        var arg = new ModArgumentList(new List<IGameArgument<string>>
        {
            modArg
        });

        _validator.Setup(v => v.CheckArgument(modArg, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
            .Returns(ArgumentValidityStatus.InvalidData);

        Assert.Throws<GameArgumentException>(() => _service.ToCommandLine(arg, "MODLIST", string.Empty));
    }

    [Fact]
    public void TestModListHasInvalidArg2_Throws()
    {
        var modArg = new InvalidModArg();
        var arg = new ModArgumentList(new List<IGameArgument<string>>
        {
            modArg
        });

        _validator.Setup(v => v.CheckArgument(modArg, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
            .Returns(ArgumentValidityStatus.InvalidData);

        Assert.Throws<GameArgumentException>(() => _service.ToCommandLine(arg, "MODLIST", string.Empty));
    }

    [Fact]
    public void TestSingleModList()
    {
        var name = "MODPATH";
        var value = "path";
        var modArg = new ModArgument("path", false);
        var arg = new ModArgumentList(new List<IGameArgument<string>>
        {
            modArg
        });

        _validator.Setup(v => v.CheckArgument(modArg, out name, out value));

        var command = _service.ToCommandLine(arg, "MODLIST", string.Empty);
        Assert.Equal("MODPATH=path", command);
    }

    [Fact]
    public void TestMultipleModList()
    {
        var name1 = "MODPATH";
        var value1 = "path";
        var modArg1 = new ModArgument("path", false);
        var name2 = "STEAMID";
        var value2 = "123";
        var modArg2 = new ModArgument("path", true);
        var arg = new ModArgumentList(new List<IGameArgument<string>>
        {
            modArg1,
            modArg2
        });

        _validator.Setup(v => v.CheckArgument(modArg1, out name1, out value1));
        _validator.Setup(v => v.CheckArgument(modArg2, out name2, out value2));

        var command = _service.ToCommandLine(arg, "MODLIST", string.Empty);
        Assert.Equal("MODPATH=path STEAMID=123", command);
    }

    private class DisabledFlag : FlagArgument
    {
        public DisabledFlag(bool dashed) : base("FLAG", false, dashed)
        {
        }
    }

    private class InvalidModArg : GameArgument<string>
    {
        public InvalidModArg() : base("value")
        {
        }

        public override ArgumentKind Kind => ArgumentKind.Flag;
        public override string Name { get; }
        public override string ValueToCommandLine()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(IGameArgument? other)
        {
            throw new NotImplementedException();
        }
    }

    private class InvalidModListArg : IGameArgument
    {
        public InvalidModListArg() 
        {
        }

        public bool Equals(IGameArgument? other)
        {
            throw new System.NotImplementedException();
        }

        public ArgumentKind Kind => ArgumentKind.ModList;
        public bool DebugArgument { get; }
        public string Name { get; }
        public object Value { get; }
        public string ValueToCommandLine()
        {
            throw new System.NotImplementedException();
        }

        public bool IsValid(out ArgumentValidityStatus reason)
        {
            throw new System.NotImplementedException();
        }
    }
}