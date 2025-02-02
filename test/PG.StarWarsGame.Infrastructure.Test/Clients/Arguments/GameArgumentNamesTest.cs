using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients.Arguments;

public class GameArgumentNamesTest
{
    [Fact]
    public void NamesAreAllUppercase()
    {
        foreach (var argumentName in GameArgumentNames.AllSupportedArgumentNames)
            Assert.True(argumentName.ToUpperInvariant().Equals(argumentName),
                $"Argument name '{argumentName}' contains lower case characters.");
    }

    [Fact]
    public void InternalNamesAreAllUppercase()
    {
        foreach (var argumentName in GameArgumentNames.AllInternalSupportedArgumentNames)
            Assert.True(argumentName.ToUpperInvariant().Equals(argumentName),
                $"Argument name '{argumentName}' contains lower case characters.");
    }
}