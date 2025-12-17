using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace AET.SteamAbstraction.Testing;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public abstract class InMemorySteamTestBase : SteamTestBase
{
    protected readonly MockFileSystem FileSystem = new();

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            serviceCollection.AddSingleton<IRegistry>(new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike));
        else
            throw new NotImplementedException("A system other than Windows is currently not supported");

        serviceCollection.AddSingleton<IFileSystem>(FileSystem);
    }
}