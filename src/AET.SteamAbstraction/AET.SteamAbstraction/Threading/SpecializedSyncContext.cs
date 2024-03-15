using System;
using System.Threading;

namespace AET.SteamAbstraction.Threading;

// From https://github.com/microsoft/vs-threading
internal readonly struct SpecializedSyncContext : IDisposable
{
    private readonly bool _initialized;
    private readonly SynchronizationContext? _prior;
    private readonly SynchronizationContext? _appliedContext;
    private readonly bool _checkForChangesOnRevert;

    private SpecializedSyncContext(SynchronizationContext? syncContext, bool checkForChangesOnRevert)
    {
        _initialized = true;
        _prior = SynchronizationContext.Current;
        _appliedContext = syncContext;
        _checkForChangesOnRevert = checkForChangesOnRevert;
        SynchronizationContext.SetSynchronizationContext(syncContext);
    }

    public static SpecializedSyncContext Apply(SynchronizationContext? syncContext, bool checkForChangesOnRevert = true)
    {
        return new SpecializedSyncContext(syncContext, checkForChangesOnRevert);
    }

    public void Dispose()
    {
        if (!_initialized) 
            return;
        // TODO Report.If(_checkForChangesOnRevert && SynchronizationContext.Current != _appliedContext);
        SynchronizationContext.SetSynchronizationContext(_prior);
    }
}