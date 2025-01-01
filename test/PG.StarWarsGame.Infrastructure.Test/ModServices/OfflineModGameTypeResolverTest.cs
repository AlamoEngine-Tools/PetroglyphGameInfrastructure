using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class OfflineModGameTypeResolverTest : ModGameTypeResolverTestBase
{
    public override ModGameTypeResolver CreateResolver()
    {
        return new OfflineModGameTypeResolver(ServiceProvider);
    }
}