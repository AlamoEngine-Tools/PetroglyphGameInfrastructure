using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

internal class ArgumentValueSerializer
{
    private static readonly Dictionary<string, (Type, TypeConverter?)> SpecialTypes;

    static ArgumentValueSerializer()
    {
        SpecialTypes = new Dictionary<string, (Type, TypeConverter?)>();
        Type[] typeArray = { 
            typeof (byte),
            typeof (ushort),
            typeof (uint),
            typeof (ulong),
            typeof (float),
            typeof(double),
            typeof(bool)
        };
        foreach (var type in typeArray) 
            SpecialTypes.Add(type.FullName!, (type, null));
    }

    public string Serialize(object value)
    {
        if (value is string stringValue)
            return stringValue;
        var type = value.GetType();
        if (type.IsEnum)
        {
            if (type.IsDefined(typeof(SerializeEnumNameAttribute), false))
                return Enum.GetName(type, value) ?? string.Empty;
            if (type.IsDefined(typeof(SerializeEnumValueAttribute), false))
                return ((Enum)value).ToString("d");
            throw new InvalidOperationException();
        }
        var converter = GetConverter(type.FullName!);
        if (converter is null)
            throw new InvalidOperationException();
        return converter.ConvertToInvariantString(value) ?? string.Empty;
    }

    public string ShortenPath(IFileSystemInfo target, IDirectoryInfo gameDir)
    {
        var fileSystem = target.FileSystem;

        var gamePath = PathNormalizer.Normalize(fileSystem.Path.GetFullPath(gameDir.FullName),
            PathNormalizeOptions.EnsureTrailingSeparator);

        var targetPath = PathNormalizer.Normalize(fileSystem.Path.GetFullPath(target.FullName),
            PathNormalizeOptions.EnsureTrailingSeparator);

        // It's important not to resolve, as that would give us an absolute path again.
        return PathNormalizer.Normalize(fileSystem.Path.GetRelativePathEx(gamePath, targetPath), new PathNormalizeOptions
        {
            TrailingDirectorySeparatorBehavior = TrailingDirectorySeparatorBehavior.Trim,
            UnifyCase = UnifyCasingKind.UpperCase, 
            UnifyDirectorySeparators = true
        });
    }

    private static TypeConverter? GetConverter(string typeName)
    {
        if (!SpecialTypes.TryGetValue(typeName, out var tuple))
            return null;
        var converter = tuple.Item2;
        if (converter != null)
            return converter;
        var type = tuple.Item1;
        SpecialTypes[typeName] = (type, converter = TypeDescriptor.GetConverter(type));
        return converter;
    }
}