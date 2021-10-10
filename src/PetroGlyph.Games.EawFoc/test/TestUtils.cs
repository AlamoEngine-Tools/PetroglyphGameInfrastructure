using System.Runtime.InteropServices;

namespace PetroGlyph.Games.EawFoc.Test
{
    internal static class TestUtils
    {
        internal static bool IsUnixLikePlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}
