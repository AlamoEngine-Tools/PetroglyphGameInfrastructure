namespace PetroGlyph.Games.EawFoc.Clients.Arguments
{
    public interface IGameArgument<T> : IGameArgument
    {
        new T Value { get; }
    }

    public interface IGameArgument
    {
        bool DebugArgument { get; }

        string Name { get; }

        object Value { get; }

        string ToCommandLine();
    }
}