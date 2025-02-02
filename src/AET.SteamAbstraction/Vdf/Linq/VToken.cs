using AET.SteamAbstraction.Vdf.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AET.SteamAbstraction.Vdf.Linq;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal abstract class VToken : IVEnumerable<VToken>
{ 
    public abstract void WriteTo(VdfWriter writer);

    public abstract VTokenType Type { get; }

    IVEnumerable<VToken> IVEnumerable<VToken>.this[object key] => this[key]!;

    public static bool DeepEquals(VToken? t1, VToken? t2)
    {
        return t1 == t2 || (t1 != null && t2 != null && t1.DeepEquals(t2));
    }

    public abstract VToken DeepClone();

    public virtual VToken? this[object key]
    {
        get => throw new InvalidOperationException($"Cannot access child value on {GetType()}.");
        set => throw new InvalidOperationException($"Cannot set child value on {GetType()}.");
    }

    public virtual T? Value<T>(object key)
    {
        var token = this[key];
        return token == null ? default : token.Convert<VToken, T>();
    }

    public virtual IEnumerable<VToken> Children()
    {
        return [];
    }

    public IEnumerable<T> Children<T>() where T : VToken
    {
        return Children().OfType<T>();
    }

    protected abstract bool DeepEquals(VToken node);

    public override string ToString()
    {
        using var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        var vdfTextWriter = new VdfTextWriter(stringWriter);
        WriteTo(vdfTextWriter);
        return stringWriter.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<VToken>)this).GetEnumerator();
    }

    IEnumerator<VToken> IEnumerable<VToken>.GetEnumerator()
    {
        return Children().GetEnumerator();
    }
}