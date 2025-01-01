using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class GameArgumentTest : GameArgumentTestBase
{
    [Fact]
    public void TestArgValid()
    {
        var arg = new ValidatingTestArgument(true, ValidStringValue);

        var valid = arg.IsValid(out var reason);

        Assert.True(valid, $"Validity is {reason} for argument {arg}");
        Assert.Equal(ArgumentValidityStatus.Valid, reason);
    }

    [Fact]
    public void TestArgInvalidCustom()
    {
        var arg = new ValidatingTestArgument(false, ValidStringValue);

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

    [Theory]
    [MemberData(nameof(GetValidArguments))]
    public void IsValid_ArgumentValid(GameArgument arg)
    {
        Assert.True(arg.IsValid(out var reason));
        Assert.Equal(ArgumentValidityStatus.Valid, reason);
    }

    [Theory]
    [MemberData(nameof(GetInvalidArguments))]
    public void IsValid_ArgumentNotValid(GameArgument arg, ArgumentValidityStatus expectedReason)
    {
        Assert.False(arg.IsValid(out var reason));
        Assert.Equal(expectedReason, reason);
    }

    public class ValidatingTestArgument(bool isValueValid, string value)
        : GameArgument(TestHelpers.GetRandom(GameArgumentNames.AllInternalSupportedArgumentNames), value)
    {
        private protected override bool IsDataValid()
        {
            return isValueValid;
        }
    }
}