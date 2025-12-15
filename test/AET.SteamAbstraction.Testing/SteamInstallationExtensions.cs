using System;
using System.IO.Abstractions;
using Testably.Abstractions.Testing;

namespace AET.SteamAbstraction.Testing;

public static class SteamInstallationExtensions
{
    public static ITestingSteamInstallation Steam(this IFileSystem fs, IServiceProvider serviceProvider)
    {
        return new TestingSteamInstallationImpl(fs, serviceProvider);
    }
    
    
    extension(ITestingSteamRegistry)
    {
        public static ITestingSteamRegistry Create(IFileSystem fileSystem, IServiceProvider serviceProvider)
        {
            return new TestingSteamRegistryImpl(fileSystem, serviceProvider);
        }
    }
}