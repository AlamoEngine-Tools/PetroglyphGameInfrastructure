namespace PetroGlyph.Games.EawFoc.Clients.Arguments
{
    public abstract class GameArgument<T> : IGameArgument<T> where T : notnull
    {
        public virtual bool DebugArgument => false;
        public abstract string Name { get; }
        public T Value { get; }

        object IGameArgument.Value => Value;

        protected GameArgument(T value)
        {
            Value = value;
        }

        public abstract string ToCommandLine();
    }
}