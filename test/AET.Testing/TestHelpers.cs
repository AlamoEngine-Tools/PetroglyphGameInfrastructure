using System;
using System.IO;

namespace AET.Testing;

public class TestHelpers
{
    public static Stream GetEmbeddedResource(Type type, string path)
    {
        var assembly = type.Assembly;
        var resourcePath = $"{assembly.GetName().Name}.Resources.{path}";
        return assembly.GetManifestResourceStream(resourcePath) ??
               throw new IOException($"Could not find embedded resource: '{resourcePath}'");
    }

    public static byte[] GetEmbeddedResourceAsByteArray(Type type, string path)
    {
        using var stream = GetEmbeddedResource(type, path);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        ms.Position = 0;
        return ms.ToArray();
    }
}