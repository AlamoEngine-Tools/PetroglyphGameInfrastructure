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

        // We don't trust the modtype of the modReference.
        if (_modGameTypeResolver.IsDefinitelyNotCompatibleToGame(modReference, game.Type))
            throw new ModException(modReference.ModReference,
                $"The mod '{modReference.ModReference.Identifier}' at location '{modDir}' is not compatible to game '{game}'");

        var isWorkshop = modReference.ModReference.Type == ModType.Workshops;

        var modinfo = modReference.Modinfo;

        if (modinfo == null)
        {
            var name = GetModName(modReference.ModReference, culture);
            return new Mod(game, modReference.ModReference.Identifier, modDir, isWorkshop, name, _serviceProvider);
        }

        modinfo.Validate();
        return new Mod(game, modReference.ModReference.Identifier, modDir, isWorkshop, modinfo, _serviceProvider);
    }

    public IVirtualMod CreateVirtualMod(IGame game, IModinfo virtualModInfo)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        if (virtualModInfo == null) 
            throw new ArgumentNullException(nameof(virtualModInfo));

        virtualModInfo.Validate();

        var virtualModRef = ModReferenceBuilder.CreateVirtualModIdentifier(virtualModInfo);
        return new VirtualMod(game, virtualModRef.Identifier, virtualModInfo, _serviceProvider);
    }

    private string GetModName(IModReference modReference, CultureInfo culture)
    {
        var name = _nameResolver.ResolveName(modReference, culture);
        if (string.IsNullOrEmpty(name))
            throw new ModException(modReference, "Unable to create a mod with an empty name.");
        return name!;
    }
}