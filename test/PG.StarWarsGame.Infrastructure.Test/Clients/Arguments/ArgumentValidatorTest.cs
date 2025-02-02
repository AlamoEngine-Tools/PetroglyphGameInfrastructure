using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients.Arguments;

public class ArgumentValidatorTest : GameArgumentTestBase
{
    [Theory]
    [MemberData(nameof(GetValidArguments))]
    public void IsValid_ArgumentValid(GameArgument arg)
    {
        Assert.Equal(ArgumentValidityStatus.Valid, ArgumentValidator.Validate(arg));
    }
    
    [Theory]
    [MemberData(nameof(GetInvalidArguments))]
    public void IsValid_ArgumentNotValid(GameArgument arg, ArgumentValidityStatus expectedReason)
    {
        Assert.Equal(expectedReason, ArgumentValidator.Validate(arg));
    }
}