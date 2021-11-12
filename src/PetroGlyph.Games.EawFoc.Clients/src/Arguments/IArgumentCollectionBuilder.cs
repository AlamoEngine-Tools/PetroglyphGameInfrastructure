namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public interface IArgumentCollectionBuilder
{
    void Add(IGameArgument argument);

    bool Remove(IGameArgument argument);

    void AddAll(ArgumentCollection argumentCollection);

    ArgumentCollection Build();
}