using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using Sklavenwalker.CommonUtilities.FileSystem;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Attribute to indicate that an enum shall be serialized by its name.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class SerializeEnumNameAttribute : Attribute { }

/// <summary>
/// Attribute to indicate that an enum shall be serialized by its underlying value.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class SerializeEnumValueAttribute : Attribute { }

internal class ArgumentValueSerializer
{
    private readonly IPathHelperService? _pathHelper;

    private static readonly Dictionary<string, (Type, TypeConverter?)> SpecialTypes;

    public ArgumentValueSerializer()
    {
    }

    internal ArgumentValueSerializer(IPathHelperService pathHelper)
    {
        _pathHelper = pathHelper;
    }

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
        var pathHelper = _pathHelper ?? new PathHelperService(gameDir.FileSystem);

        var gamePath = pathHelper.EnsureTrailingSeparator(
            pathHelper.NormalizePath(gameDir.FullName, PathNormalizeOptions.Full));
        var targetPath = pathHelper.EnsureTrailingSeparator(
            pathHelper.NormalizePath(target.FullName, PathNormalizeOptions.Full));

        // It's important not to resolve, as that would give us an absolute path again.
        return pathHelper.NormalizePath(pathHelper.GetRelativePath(gamePath, targetPath), PathNormalizeOptions.FullNoResolve);
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