﻿using System;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Base implementation for a typed <see cref="IGameArgument{T}"/>.
/// </summary>
/// <typeparam name="T">The non-nullable type of the argument.</typeparam>
public abstract class GameArgument<T> : IGameArgument<T> where T : notnull
{
    /// <inheritdoc/>
    public abstract ArgumentKind Kind { get; }

    /// <inheritdoc/>
    public bool DebugArgument { get; }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public T Value { get; }

    object IGameArgument.Value => Value;

    /// <summary>
    /// Initializes a new argument with a given value and the inforation if this argument is for debug mode only.
    /// </summary>
    /// <param name="value">The argument's value.</param>
    /// <param name="isDebug">Indicates whether this instance if for debug mode only.</param>
    protected GameArgument(T value, bool isDebug = false)
    {
        Requires.NotNullAllowStructs(value, nameof(value));
        Value = value;
        DebugArgument = isDebug;
    }

    /// <inheritdoc/>
    public abstract string ValueToCommandLine();

    /// <summary>
    /// This method shall only perform semantical checks on the <see cref="Value"/> property.
    /// <para>If this method returns <see langword="false"/>, <see cref="IsValid(out ArgumentValidityStatus)"/>
    /// with return <langword cref="false"/> with reason <see cref="ArgumentValidityStatus.InvalidData"/>.</para>
    /// <para>
    /// This methods gets called in the sequence of <see cref="IsValid"/>. Returns <see langword="true"/> by default.
    /// </para>
    /// </summary>
    /// <returns><see langword="true"/> if the data is valid; <see langword="false"/> otherwise.</returns>
    protected virtual bool IsDataValid()
    {
        return true;
    }

    /// <inheritdoc/>
    public bool IsValid(out ArgumentValidityStatus reason)
    {
        return IsValid(new ArgumentValidator(), out reason);
    }


    internal bool IsValid(IArgumentValidator validator, out ArgumentValidityStatus reason)
    {
        try
        {
            reason = validator.CheckArgument(this, out _, out _);
            if (reason != ArgumentValidityStatus.Valid)
                return false;
            if (IsDataValid())
                return true;
            reason = ArgumentValidityStatus.InvalidData;
            return false;
        }
        catch (Exception)
        {
            reason = ArgumentValidityStatus.InvalidData;
            return false;
        }
    }

    /// <inheritdoc/>
    public abstract bool Equals(IGameArgument? other);
}