using System;
using System.Globalization;
using System.IO;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;

namespace PG.StarWarsGame.Infrastructure.Services;

internal class ModFactory(IServiceProvider serviceProvider) : IModFactory
{
    private readonly IModNameResolver _nameResolver = serviceProvider.GetRequiredService<IModNameResolver>();
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IModGameTypeResolver _modGameTypeResolver = serviceProvider.GetRequiredService<IModGameTypeResolver>();

    public IPhysicalMod CreatePhysicalMod(IGame game, DetectedModReference modReference, CultureInfo culture)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        if (modReference == null) 
            throw new ArgumentNullException(nameof(modReference));
        if (culture == null) 
            throw new ArgumentNullException(nameof(culture));

        if (modReference.ModReference.Type == ModType.Virtual)
            throw new NotSupportedException("Cannot create virtual mod.");

        var modDir = modReference.Directory;

        modDir.Refresh();
        if (!modDir.Exists)
            throw new DirectoryNotFoundException($"Mod installation not found at '{modReference.Directory.FullName}'.");

        return null!;


        //_modGameTypeResolver.IsDefinitelyNotCompatibleToGame(modDir, modReference.ModReference.Type, modReference.Modinfo, game.Type);

        //// We don't trust the modtype of the modReference.
        //ThrowIfDefinitelyNotGameType(game.Type, modDir, modReference.ModReference.Type, modReference.Modinfo, modReference);

        //var isWorkshop = modReference.ModReference.Type == ModType.Workshops;

        //if (modinfo == null)
        //{
        //    var name = GetModName(modReference, culture);
        //    return new Mod(game, directory, isWorkshop, name, _serviceProvider);
        //}

        //modinfo.Validate();
        //return new Mod(game, directory, isWorkshop, modinfo, _serviceProvider);
    }

    public IVirtualMod CreateVirtualMod(IGame game, IModinfo virtualModInfo)
    {
        throw new NotImplementedException();
    }

    public IVirtualMod CreateVirtualMod(IGame game, string name, IModDependencyList dependencies)
    {
        throw new NotImplementedException();
    }

    private string GetModName(IModReference modReference, CultureInfo culture)
    {
        var name = _nameResolver.ResolveName(modReference, culture);
        if (string.IsNullOrEmpty(name))
            throw new ModException(modReference, "Unable to create a mod with an empty name.");
        return name!;
    }
}