namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

internal interface IArgumentValidator
{
    ArgumentValidityStatus CheckArgument(IGameArgument argument, out string name, out string value);
}