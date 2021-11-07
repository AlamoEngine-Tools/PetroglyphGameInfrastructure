using System;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Processes;

namespace PetroGlyph.Games.EawFoc.Clients;

public sealed class DefaultClient : ClientBase
{
    public DefaultClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    
    protected override IGameProcessLauncher GetGameLauncherService()
    { 
        return ServiceProvider.GetService<IGameProcessLauncher>() ?? new DefaultGameProcessLauncher();
    }
}