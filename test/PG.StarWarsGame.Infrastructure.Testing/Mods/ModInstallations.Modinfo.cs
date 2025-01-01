using System;
using System.IO;
using EawModinfo.File;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Mods;

public static partial class ModInstallations
{
    public static IModinfoFile InstallInvalidModinfoFile(this IPhysicalMod mod, string? variantSubFileName = null)
    {
        return InstallModinfoFile(mod, stream => { stream.WriteByte(0); }, variantSubFileName);
    }
    
    public static IModinfoFile InstallModinfoFile(
        this IPhysicalMod mod, 
        IModinfo modinfo,
        string? variantSubFileName = null)
    {
        return InstallModinfoFile(mod, modinfo.ToJson, variantSubFileName);
    }

    private static IModinfoFile InstallModinfoFile(
        this IPhysicalMod mod,
        Action<Stream> writeAction,
        string? variantSubFileName)
    {
        var dir = mod.Directory;
        dir.Create();

        var fs = dir.FileSystem;

        var modinfoFilePath = fs.Path.Combine(dir.FullName,
            variantSubFileName != null
                ? $"{variantSubFileName}-modinfo.json"
                : "modinfo.json");

        using var fileStream = fs.FileStream.New(modinfoFilePath, FileMode.Create);
        writeAction(fileStream);

        var fileInfo = fs.FileInfo.New(modinfoFilePath);

        if (variantSubFileName is null)
            return new ModinfoVariantFile(fileInfo);
        return new ModinfoVariantFile(fileInfo);
    }

}