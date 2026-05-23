using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities;

namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

internal sealed class GameProcess : DisposableObject, IGameProcess
{
    private int _closedFlag;

    private EventHandler? _closingHandler;

    public event EventHandler? Closed
    {
        add
        {
            // Execute event right away if the process was already closed.
            if (Volatile.Read(ref _closedFlag) != 0)
                value?.Invoke(this, EventArgs.Empty);
            else
                _closingHandler += value;
        }
        remove => _closingHandler -= value;
    }

    public GameProcessInfo ProcessInfo { get; }

    // To avoid complexity this property does not query the underlying process but just "trusts"
    // on the OnClosed function to set the value when the process was terminated.
    // This means this property has theoretically a race condition with the game's process.
    public GameProcessState State => Volatile.Read(ref _closedFlag) != 0 ? GameProcessState.Closed : GameProcessState.Running;

    internal Process Process { get; }

    public GameProcess(Process process, GameProcessInfo info)
    {
        Process = process ?? throw new ArgumentNullException(nameof(process));
        ProcessInfo = info ?? throw new ArgumentNullException(nameof(info));
        RegisterExitEvent(process);
    }

    public void Exit()
    {
        if (IsDisposed)
            throw new InvalidOperationException("No process is associated with this object."); // Replicate .NET behavior
        if (State == GameProcessState.Closed)
            return;
        try
        {
            Process.Kill();
            Process.WaitForExit();
        }
        catch (Exception e) when (e is InvalidOperationException or Win32Exception)
        {
        }

        // Process.Exited is dispatched asynchronously on a ThreadPool work item, so
        // WaitForExit can return before OnClosed has been invoked. Drive it synchronously
        // here; OnClosed is idempotent via the Interlocked guard.
        OnClosed(this, EventArgs.Empty);
    }

    public Task WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return State == GameProcessState.Closed ? Task.CompletedTask : Process.WaitForExitAsync(cancellationToken);
    }

    protected override void DisposeResources()
    {
        Process.Dispose();
        base.DisposeResources();
    }

    private void RegisterExitEvent(Process process)
    {
        Process.EnableRaisingEvents = true;
        Process.Exited += OnClosed;
        if (process.HasExited)
            OnClosed(this, EventArgs.Empty);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (Interlocked.Exchange(ref _closedFlag, 1) != 0)
            return;
        Process.Exited -= OnClosed;
        _closingHandler?.Invoke(this, EventArgs.Empty);
    }
}