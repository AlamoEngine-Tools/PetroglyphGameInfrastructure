using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;

namespace AET.SteamAbstraction.Testing.TestBases;

public abstract class InMemorySteamTestBase : SteamTestBase
{
    protected readonly MockFileSystem FileSystem = new();

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IRegistry>(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike)
            : new InMemoryRegistry());

        serviceCollection.AddSingleton<IFileSystem>(FileSystem);
    }
}