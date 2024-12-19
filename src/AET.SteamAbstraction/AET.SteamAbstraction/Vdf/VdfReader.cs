using System;

namespace AET.SteamAbstraction.Vdf;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal abstract class VdfReader(VdfSerializerSettings settings) : IDisposable
{
    public VdfSerializerSettings Settings { get; } = settings;

    public bool CloseInput { get; set; } = true;

    public string Value { get; set; } = null!;

    protected internal State CurrentState { get; protected set; } = State.Start;

    protected VdfReader() : this(VdfSerializerSettings.Default)
    {
    }

    public abstract bool ReadToken();

    void IDisposable.Dispose()
    {
        if (CurrentState == State.Closed)
            return;

        Close();
    }

    public virtual void Close()
    {
        CurrentState = State.Closed;
        Value = null!;
    }

    protected internal enum State
    {
        Start = 0,
        Property = 1,
        Object = 2,
        Comment = 3,
        Conditional = 4,
        Finished = 5,
        Closed = 6
    }
}