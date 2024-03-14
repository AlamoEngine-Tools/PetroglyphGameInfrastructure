using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities;
#if !NET
#endif

namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

internal sealed class GameProcess : IGameProcess
{
    private volatile bool _closed;

    private EventHandler? _closingHandler;

    public event EventHandler? Closed
    {
        add
        {
            // Execute event right away if the process was already closed.
            if (_closed)
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
    public GameProcessState State => _closed ? GameProcessState.Closed : GameProcessState.Running;

    internal Process Process { get; }

    public GameProcess(Process process, GameProcessInfo info)
    {
        Process = process ?? throw new ArgumentNullException(nameof(process));
        ProcessInfo = info ?? throw new ArgumentNullException(nameof(info));
        RegisterExitEvent(process);
    }

    public void Exit()
    {
        if (State == GameProcessState.Closed)
            return;
        Process.Kill();
    }

    public Task WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        return State == GameProcessState.Closed ? Task.CompletedTask : Process.WaitForExitAsync(cancellationToken);
    }

    private void RegisterExitEvent(Process process)
    {
        try
        {
            Process.EnableRaisingEvents = true;
        }
        catch (InvalidOperationException)
        {
            if (process.HasExited)
                _closed = true;
            throw;
        }
        Process.Exited += OnClosed;
        if (process.HasExited)
        {
            _closed = true;
            Process.Exited -= OnClosed;
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _closed = true;
        Process.Exited -= OnClosed;
        _closingHandler?.Invoke(this, EventArgs.Empty);
    }
}