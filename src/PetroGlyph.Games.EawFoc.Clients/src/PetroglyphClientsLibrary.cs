using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Processes;

namespace PetroGlyph.Games.EawFoc.Clients
{
    /// <summary>
    /// Provides initialization routines for this library.
    /// </summary>
    public class PetroglyphClientsLibrary
    {
        /// <summary>
        /// Adds services provided by this library to the given <paramref name="serviceCollection"/>
        /// so that the library can be used in client applications. 
        /// </summary>
        /// <param name="serviceCollection">The service collection to be filled.</param>
        public static void InitializeLibraryWithDefaultServices(IServiceCollection serviceCollection)
        {
            // Singletons
            serviceCollection.AddTransient<IProcessHelper>(_ => new ProcessHelper());
        }
    }
}
