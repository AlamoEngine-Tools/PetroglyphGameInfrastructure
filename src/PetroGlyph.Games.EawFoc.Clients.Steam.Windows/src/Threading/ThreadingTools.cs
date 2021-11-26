using System.Threading;

namespace PetroGlyph.Games.EawFoc.Clients.Steam.Threading;

// From https://github.com/microsoft/vs-threading
internal static class ThreadingTools
{
    public static SpecializedSyncContext Apply(this SynchronizationContext? syncContext,
        bool checkForChangesOnRevert = true)
    {
        return SpecializedSyncContext.Apply(syncContext, checkForChangesOnRevert);
    }
}