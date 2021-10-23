namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public abstract class DebugGameArgument<T> : GameArgument<T> where T : notnull
{
    public sealed override bool DebugArgument => true;

    protected DebugGameArgument(T value) : base(value)
    {
    }
}