using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Services.Detection;

namespace PetroGlyph.Games.EawFoc
{
    /// <summary>
    /// Provides initialization routines for this library.
    /// </summary>
    public static class PetroglyphGameInfrastructureLibrary
    {
        /// <summary>
        /// Adds services provided by this library to the given <paramref name="serviceCollection"/>
        /// so that the library can be used in client applications. 
        /// </summary>
        /// <param name="serviceCollection">The service collection to be filled.</param>
        public static void InitializeLibraryWithDefaultServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IModIdentifierBuilder>(sp => new ModIdentifierBuilder(sp));
        }
    }
}
