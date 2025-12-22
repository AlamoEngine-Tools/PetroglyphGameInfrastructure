using System;
using System.IO;

namespace AET.Testing;

/// <summary>
/// Provides common helper methods useful creating test code.
/// </summary>
public class TestingHelpers
{
    /// <summary>
    /// Retrieves an embedded resource stream from the specified assembly and path.
    /// </summary>
    /// <remarks>
    /// Embedded resources are expected to be located in the "Resources" folder of the assembly.
    /// </remarks>
    /// <param name="type">A <see cref="Type"/> from the assembly containing the embedded resource.</param>
    /// <param name="path">The relative path of the embedded resource within the assembly.</param>
    /// <returns>A <see cref="Stream"/> representing the embedded resource.</returns>
    /// <exception cref="IOException">Thrown when the specified embedded resource cannot be found.</exception>
    public static Stream GetEmbeddedResource(Type type, string path)
    {
        var assembly = type.Assembly;
        var resourcePath = $"{assembly.GetName().Name}.Resources.{path}";
        return assembly.GetManifestResourceStream(resourcePath) ??
               throw new IOException($"Could not find embedded resource: '{resourcePath}'");
    }

    /// <summary>
    /// Retrieves an embedded resource as a byte array from the specified assembly and path.
    /// </summary>
    /// <remarks>
    /// Embedded resources are expected to be located in the "Resources" folder of the assembly.
    /// </remarks>
    /// <param name="type">A <see cref="Type"/> from the assembly containing the embedded resource.</param>
    /// <param name="path">The relative path of the embedded resource within the assembly.</param>
    /// <returns>A byte array containing the content of the embedded resource.</returns>
    /// <exception cref="IOException">Thrown when the specified embedded resource cannot be found.</exception>
    public static byte[] GetEmbeddedResourceAsByteArray(Type type, string path)
    {
        using var stream = GetEmbeddedResource(type, path);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        ms.Position = 0;
        return ms.ToArray();
    }
}