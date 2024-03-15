using System.Threading;

namespace AET.SteamAbstraction.Threading;

// From https://github.com/microsoft/vs-threading
internal static class ThreadingTools
{
    public static SpecializedSyncContext Apply(this SynchronizationContext? syncContext,
        bool checkForChangesOnRevert = true)
    {
        return SpecializedSyncContext.Apply(syncContext, checkForChangesOnRevert);
    }
}