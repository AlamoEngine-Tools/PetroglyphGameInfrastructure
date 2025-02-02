using System;
using System.Globalization;
using System.IO;
using AET.Modinfo.Spec;
using AET.Modinfo.Utilities;
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

    public IPhysicalMod CreatePhysicalMod(IGame game, DetectedModReference detectedMod, CultureInfo culture)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        if (detectedMod == null) 
            throw new ArgumentNullException(nameof(detectedMod));
        if (culture == null) 
            throw new ArgumentNullException(nameof(culture));

        if (detectedMod.ModReference.Type == ModType.Virtual)
            throw new NotSupportedException("Cannot create virtual mod.");

        var modDir = detectedMod.Directory;

        modDir.Refresh();
        if (!modDir.Exists)
            throw new DirectoryNotFoundException($"Mod installation not found at '{detectedMod.Directory.FullName}'.");

        // We don't trust the modtype of the detectedMod.
        if (_modGameTypeResolver.IsDefinitelyNotCompatibleToGame(detectedMod, game.Type))
            throw new ModException(detectedMod.ModReference,
                $"The mod '{detectedMod.ModReference.Identifier}' at location '{modDir}' is not compatible to game '{game}'");

        var isWorkshop = detectedMod.ModReference.Type == ModType.Workshops;

        var modinfo = detectedMod.Modinfo;

        if (modinfo == null)
        {
            var name = GetModName(detectedMod, culture);
            return new Mod(game, detectedMod.ModReference.Identifier, modDir, isWorkshop, name, _serviceProvider);
        }

        modinfo.Validate();
        return new Mod(game, detectedMod.ModReference.Identifier, modDir, isWorkshop, modinfo, _serviceProvider);
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

    private string GetModName(DetectedModReference detectedMod, CultureInfo culture)
    {
        var name = _nameResolver.ResolveName(detectedMod, culture);
        if (string.IsNullOrEmpty(name))
            throw new ModException(detectedMod.ModReference, "Unable to create a mod with an empty name.");
        return name!;
    }
}