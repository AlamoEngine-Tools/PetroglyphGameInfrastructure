using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using System;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Represents an argument for a Petroglyph Star Wars game.
/// </summary>
public abstract class GameArgument
{
    /// <summary>
    /// Gets a value indicating whether this argument can be used in debug mode only.
    /// </summary>
    public bool DebugArgument { get; }

    /// <summary>
    /// Gets the name of the argument.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the value of the argument.
    /// </summary>
    public object Value { get; }

    internal virtual bool HasPathValue => false;

    private protected GameArgument(string name, object value, bool isDebug = false)
    {
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(name);
        Name = name.ToUpperInvariant();
        Value = value ?? throw new ArgumentNullException(nameof(value));
        DebugArgument = isDebug;
    }

    // Internal, because the returned value is unsafe by design.
    // We don't want to easily enable users to build unsafe command lines on their own.
    // Data validation is done in ArgumentCommandLineBuilder
    internal virtual string ValueToCommandLine()
    {
        return ArgumentValueSerializer.Serialize(Value);
    }

    /// <summary>
    /// This method shall only perform semantic checks on the <see cref="Value"/> property.
    /// <para>If this method returns <see langword="false"/>, <see cref="IsValid(out ArgumentValidityStatus)"/>
    /// with return <see langword="false"/> with reason <see cref="ArgumentValidityStatus.InvalidData"/>.</para>
    /// <para>
    /// This method gets called in the sequence of <see cref="IsValid"/>. Returns <see langword="true"/> by default.
    /// </para>
    /// </summary>
    /// <returns><see langword="true"/> if the data is valid; <see langword="false"/> otherwise.</returns>
    private protected virtual bool IsDataValid()
    {
        return true;
    }

    /// <summary>
    /// Checks whether this argument has correct data. This checks the <see cref="Name"/> as well as <see cref="Value"/>.
    /// </summary>
    /// <param name="reason">Detailed status of the validation.</param>
    /// <returns><see langword="true"/> when the argument is valid; <see langword="false"/> otherwise.</returns>
    public bool IsValid(out ArgumentValidityStatus reason)
    {
        if (!IsDataValid())
        {
            reason = ArgumentValidityStatus.InvalidData;
            return false;
        }

        reason = ArgumentValidator.Validate(this);
        if (reason != ArgumentValidityStatus.Valid)
            return false;

        reason = ArgumentValidityStatus.Valid;
        return true;
    }

    /// <summary>
    /// Compares two <see cref="GameArgument"/> for equality.
    /// </summary>
    /// <param name="other">The game argument to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the two instances represent the argument; otherwise, <see langword="false"/>.</returns>
    public bool Equals(GameArgument? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (GetType() != other.GetType())
            return false;
        return Name.Equals(other.Name) && EqualsValue(other);
    }

    /// <summary>
    /// Compares two <see cref="GameArgument"/> for equality.
    /// </summary>
    /// <param name="obj">The game argument to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the two instances represent the argument; otherwise, <see langword="false"/>.</returns>
    public sealed override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        return ReferenceEquals(this, obj) || Equals((GameArgument)obj);
    }

    /// <summary>
    /// Gets the hash code for the game argument.
    /// </summary>
    /// <returns>The hash value generated for this game argument.</returns>
    public sealed override int GetHashCode()
    {
        return HashCode.Combine(GetType().FullName, Name, ValueHash());
    }

    /// <summary>
    /// Returns a textual representation of the game argument.
    /// </summary>
    /// <remarks>
    /// The returned value must not be used to build a command line.
    /// </remarks>
    /// <returns>A textual representation of the game argument.</returns>
    public sealed override string ToString()
    {
        return $"{Name}:{Value}";
    }

    private protected virtual bool EqualsValue(GameArgument other)
    {
        return ValueToCommandLine().Equals(other.ValueToCommandLine());
    }

    private protected virtual int ValueHash()
    {
        return ValueToCommandLine().GetHashCode();
    }
}

/// <summary>
/// Base implementation for a typed <see cref="GameArgument{T}"/>.
/// </summary>
/// <typeparam name="T">The non-nullable type of the argument.</typeparam>
public abstract class GameArgument<T> : GameArgument where T : notnull
{
    /// <summary>
    /// Gets the value of game argument.
    /// </summary>
    public new T Value { get; }

    private protected GameArgument(string name, T value, bool isDebug = false) : base(name, value, isDebug)
    {
        Value = value;
    }
}