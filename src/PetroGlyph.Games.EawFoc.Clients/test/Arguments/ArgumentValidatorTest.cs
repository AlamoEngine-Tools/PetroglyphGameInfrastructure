using System;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ArgumentValidatorTest
{
    [Fact]
    public void TestOutVariables()
    {
        var validator = new ArgumentValidator();

        IGameArgument arg = new StringArg("VaLuE");
        validator.CheckArgument(arg, out var name, out var value);

        Assert.Equal("MAP", name);
        Assert.Equal("VaLuE", value);

        arg = new NotUppercaseNameArg();
        validator.CheckArgument(arg, out name, out _);
        Assert.Equal("MAP", name);
    }

    public static IEnumerable<object[]> GetIllegalCharData()
    {
        for (var i = 0; i < 32; i++)
        {
            if (i is '\f' or '\n' or '\r' or '\t' or '\v' )
                continue;
            yield return [$"abc{(char)i}"];
        }
        yield return ["abc?"];
        yield return ["abc*"];
        yield return ["abc:"];
        yield return ["abc|"];
        yield return ["abc>"];
        yield return ["abc<"];
        yield return ["abc&calc.exe"];
        yield return ["abc{'\"'}"];
    }

    [Theory]
    [MemberData(nameof(GetIllegalCharData))]
    public void TestHasIllegalChar(string value)
    {
        var validator = new ArgumentValidator();
        var arg = new StringArg(value);
        var reason = validator.CheckArgument(arg, out _, out _);
        Assert.True(reason == ArgumentValidityStatus.IllegalCharacter);
    }

    public static IEnumerable<object[]> GetEmptyDataValues()
    {
        yield return [new StringArg(""), true];
        yield return [new StringArg(string.Empty), true];
        yield return [new StringArg("\0"), false];
        yield return [new StringArg("abc"), false];
        yield return [new StringArg("     "), false];
    }

    [Theory]
    [MemberData(nameof(GetEmptyDataValues))]
    public void TestHasEmptyData(IGameArgument arg, bool hasEmptyData)
    {
        var validator = new ArgumentValidator();
        var reason = validator.CheckArgument(arg, out _, out _);
        Assert.Equal(hasEmptyData, reason == ArgumentValidityStatus.EmptyData);
    }

    [Fact]
    public void TestHasInvalidName()
    {
        var validator = new ArgumentValidator();
        Assert.Equal(ArgumentValidityStatus.InvalidName, validator.CheckArgument(new InvalidNameArg(), out _, out _));
        Assert.Equal(ArgumentValidityStatus.Valid, validator.CheckArgument(new NotUppercaseNameArg(), out _, out _));
    }

    public static IEnumerable<object[]> GetSpaceTestData()
    {
        yield return [new StringArg(""), false];
        yield return [new StringArg("testvalue"), false];
        yield return [new StringArg("    "), true];
        yield return [new StringArg(" "), true];
        yield return [new StringArg("test\tvalue"), true];
        yield return [new StringArg("test\fvalue"), true];
        yield return [new StringArg("test\rvalue"), true];
        yield return [new StringArg("test\nvalue"), true];
        yield return [new StringArg("test\vvalue"), true];
        yield return [new StringArg("\0"), false];
        yield return [new StringArg("test value"), true];
        yield return [new StringArg("test\\path with space\\"), true];
        yield return [new StringArg("testvalue "), true];
        yield return [new DoubleArg(1.2), false];
    }

    [Theory]
    [MemberData(nameof(GetSpaceTestData))]
    public void TestHasSpaces(IGameArgument arg, bool hasSpace)
    {
        var validator = new ArgumentValidator();
        var reason = validator.CheckArgument(arg, out _, out _);
        Assert.Equal(hasSpace, reason == ArgumentValidityStatus.PathContainsSpaces);
    }

    [Fact]
    public void TestModList()
    {
        var validator = new ArgumentValidator();
        Assert.Equal(ArgumentValidityStatus.InvalidData, validator.CheckArgument(new InvalidModList(), out _, out _));
        Assert.Equal(ArgumentValidityStatus.Valid, validator.CheckArgument(new ModArgumentList(Array.Empty<ModArgument>()), out _, out _));
    }


    private class StringArg : NamedArgument<string>
    {
        public StringArg(string value) : base("MAP", value, false)
        {
        }

        public override string ValueToCommandLine()
        {
            return Value;
        }
    }

    private class DoubleArg : NamedArgument<double>
    {
        public DoubleArg(double value) : base("NUMBER", value, false)
        {
        }

        public override string ValueToCommandLine()
        {
            return new ArgumentValueSerializer().Serialize(Value);
        }
    }

    private class InvalidNameArg : FlagArgument
    {
        public InvalidNameArg() : base("SOME_NAME", true)
        {
        }
    }

    private class NotUppercaseNameArg : FlagArgument
    {
        public NotUppercaseNameArg() : base("map", true)
        {
        }
    }

    private class InvalidModList : IGameArgument
    {
        public bool Equals(IGameArgument? other)
        {
            throw new NotImplementedException();
        }

        public ArgumentKind Kind => ArgumentKind.ModList;
        public bool DebugArgument { get; } = false;
        public string Name => ArgumentNameCatalog.ModListArg;
        public object Value => "Some Value";
        public string ValueToCommandLine()
        {
            return Value.ToString()!;
        }

        public bool IsValid(out ArgumentValidityStatus reason)
        {
            throw new NotImplementedException();
        }
    }
}