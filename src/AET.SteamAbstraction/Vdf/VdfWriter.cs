using AET.SteamAbstraction.Vdf.Linq;
using System;
using System.Collections.Generic;

namespace AET.SteamAbstraction.Vdf;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal abstract class VdfWriter(VdfSerializerSettings settings) : IDisposable
{
    public VdfSerializerSettings Settings { get; } = settings;
    public bool CloseOutput { get; set; } = true;
    protected internal State CurrentState { get; protected set; } = State.Start;

    protected VdfWriter() : this(VdfSerializerSettings.Default) { }

    public abstract void WriteObjectStart();

    public abstract void WriteObjectEnd();

    public abstract void WriteKey(string key);

    public abstract void WriteValue(VValue value);

    public abstract void WriteComment(string text);

    public abstract void WriteConditional(IReadOnlyList<VConditional.Token> tokens);

    void IDisposable.Dispose()
    {
        if (CurrentState == State.Closed)
            return;

        Close();
    }

    public virtual void Close()
    {
        CurrentState = State.Closed;
    }

    protected internal enum State
    {
        Start = 0,
        Key = 1,
        Value = 2,
        ObjectStart = 3,
        ObjectEnd = 4,
        Comment = 5,
        Conditional = 6,
        Finished = 7,
        Closed = 8
    }
}