using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;

internal static class ArgumentValueSerializer
{
    private static readonly PathNormalizeOptions NormalizeOptions = new()
    {
        TrailingDirectorySeparatorBehavior = TrailingDirectorySeparatorBehavior.Trim,
        UnifyCase = UnifyCasingKind.UpperCase,
        UnifyDirectorySeparators = true
    };

    private static readonly Dictionary<string, (Type, TypeConverter?)> SpecialTypes;

    static ArgumentValueSerializer()
    {
        SpecialTypes = new Dictionary<string, (Type, TypeConverter?)>();
        Type[] typeArray =
        [
            typeof (byte),
            typeof (ushort),
            typeof (uint),
            typeof (ulong),
            typeof (float),
            typeof(double),
            typeof(bool)
        ];
        foreach (var type in typeArray)
            SpecialTypes.Add(type.FullName!, (type, null));
    }

    public static string Serialize(object value)
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

    public static string ShortenPath(IFileSystemInfo target, IDirectoryInfo gameDir)
    {
        var fileSystem = target.FileSystem;

        if (!fileSystem.Path.IsChildOf(gameDir.FullName, target.FullName))
            return PathNormalizer.Normalize(target.FullName, NormalizeOptions);

        return PathNormalizer.Normalize(fileSystem.Path.GetRelativePathEx(gameDir.FullName, target.FullName), NormalizeOptions);
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