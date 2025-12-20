using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;

namespace AET.SteamAbstraction.Testing.TestBases;

/// <summary>
/// Provides a base class for Steam-related tests that work with in-memory file system and registry.
/// </summary>
public abstract class InMemorySteamTestBase : SteamTestBase
{
    /// <summary>
    /// Gets the in-memory file system used for testing.
    /// </summary>
    protected readonly MockFileSystem FileSystem = new();

    /// <inheritdoc />
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IRegistry>(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike)
            : new InMemoryRegistry());

        serviceCollection.AddSingleton<IFileSystem>(FileSystem);
    }
}