using System;
using System.IO;
using AET.Modinfo.File;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

internal sealed class TestingPhysicalModInstallationImpl(ITestingGameInstallation gameInstallation, IPhysicalMod mod, IServiceProvider serviceProvider) 
    : TestingModInstallationImpl(gameInstallation, mod, serviceProvider), ITestingPhysicalModInstallation, ITestingPhysicalPlayableObjectInstallation
{
    public new ITestingGameInstallation GameInstallation { get; } = gameInstallation;

    public IPhysicalPlayableObject PlayableObject => Mod;
    
    public void InstallLanguage(ILanguageInfo language)
    {
        PlayableObjectTestingUtilities.InstallLanguage(PlayableObject, language, FileSystem);
    }

    public new IPhysicalMod Mod { get; } = mod;

    public IModinfoFile InstallInvalidModinfoFile(string? variantSubFileName = null)
    {
        return InstallModinfoFile(stream => { stream.WriteByte(0); }, variantSubFileName);
    }

    public IModinfoFile InstallModinfoFile(IModinfo modinfo, string? variantSubFileName = null)
    {
        return InstallModinfoFile(modinfo.ToJson, variantSubFileName);
    }

    private IModinfoFile InstallModinfoFile(Action<Stream> writeAction, string? variantSubFileName)
    {
        var dir = Mod.Directory;
        dir.Create();

        var modinfoFilePath = FileSystem.Path.Combine(dir.FullName,
            variantSubFileName != null
                ? $"{variantSubFileName}-modinfo.json"
                : "modinfo.json");

        using var fileStream = FileSystem.FileStream.New(modinfoFilePath, FileMode.Create);
        writeAction(fileStream);

        var fileInfo = FileSystem.FileInfo.New(modinfoFilePath);

        if (variantSubFileName is null)
            return new MainModinfoFile(fileInfo);
        return new ModinfoVariantFile(fileInfo);
    }
}