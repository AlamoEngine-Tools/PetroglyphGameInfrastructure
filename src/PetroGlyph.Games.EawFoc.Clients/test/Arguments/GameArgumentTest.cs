using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class GameArgumentTest : GameArgumentTestBase
{
    [Theory]
    [InlineData(ArgumentKind.Flag)]
    [InlineData(ArgumentKind.DashedFlag)]
    [InlineData(ArgumentKind.KeyValue)]
    [InlineData(ArgumentKind.ModList)]
    public void TestArgValid(ArgumentKind kind)
    {
        var value = ValidStringValue;
        if (kind == ArgumentKind.ModList)
            value = string.Empty;

        var arg = new ValidatingTestArgument(true, kind, value);

        var valid = arg.IsValid(out var reason);

        if (!valid) 
            arg.IsValid(out reason);

        Assert.True(valid, $"Validity is {reason} for argument '{arg.Kind}' {arg.Name}");
        Assert.Equal(ArgumentValidityStatus.Valid, reason);
    }

    [Theory]
    [MemberData(nameof(GetIllegalCharacterGameArgs))]
    public void TestArgInvalid(GameArgument arg)
    {
        var valid = arg.IsValid(out var reason);

        Assert.False(valid);
        Assert.Equal(ArgumentValidityStatus.IllegalCharacter, reason);
    }

    [Theory]
    [InlineData(ArgumentKind.Flag)]
    [InlineData(ArgumentKind.DashedFlag)]
    [InlineData(ArgumentKind.KeyValue)]
    [InlineData(ArgumentKind.ModList)]
    public void TestArgInvalidCustom(ArgumentKind kind)
    {
        var value = ValidStringValue;
        if (kind == ArgumentKind.ModList)
            value = string.Empty;

        var arg = new ValidatingTestArgument(false, kind, value);

        var valid = arg.IsValid(out var reason);

        Assert.False(valid);
        Assert.Equal(ArgumentValidityStatus.InvalidData, reason);
    }

    [Fact]
    public void NameIsAlwaysUppercased()
    {
        var arg = new LowerCaseNameArg();
        Assert.Equal(arg.Name.ToUpperInvariant(), arg.Name);
    }

    [Fact]
    public void ModListArgKindRequiresEmptyStringValue_InvalidTest()
    {
        var invalidArg = new ValidatingTestArgument(true, ArgumentKind.ModList, "notEmpty");
        var valid = invalidArg.IsValid(out var reason);
        Assert.False(valid);
        Assert.Equal(ArgumentValidityStatus.InvalidData, reason);
    }

    [Fact]
    public void ModListArgKindRequiresEmptyStringValue_ValidTest()
    {
        var invalidArg = new ValidatingTestArgument(true, ArgumentKind.ModList, string.Empty);
        var valid = invalidArg.IsValid(out var reason);
        Assert.True(valid, $"Validity is {reason}");
        Assert.Equal(ArgumentValidityStatus.Valid, reason);
    }

    public class ValidatingTestArgument(bool isValid, ArgumentKind kind, string value)
        : GameArgument(TestHelpers.GetRandom(GameArgumentNames.AllInternalSupportedArgumentNames), value)
    {
        public override ArgumentKind Kind => kind;

        private protected override bool IsDataValid()
        {
            return isValid;
        }
    }
}