using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;
 
/// <summary>
/// Default client implementation for non-Steam games.
/// </summary>
public sealed class DefaultClient : ClientBase
{
    /// <summary>
    /// This instance supports all platforms except for <see cref="GamePlatform.SteamGold"/>.
    /// </summary>
    public override ISet<GamePlatform> SupportedPlatforms => new HashSet<GamePlatform>
    {
        GamePlatform.Disk, GamePlatform.DiskGold, GamePlatform.GoG, GamePlatform.Origin
    };

    /// <summary>
    /// Creates a new client instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public DefaultClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Gets an <see cref="IGameProcessLauncher"/> instance registered to the service provider or uses the default launcher instance.
    /// </summary>
    /// <returns>The <see cref="IGameProcessLauncher"/> instance.</returns>
    protected internal override IGameProcessLauncher GetGameLauncherService()
    { 
        return ServiceProvider.GetRequiredService<IGameProcessLauncher>();
    }
}