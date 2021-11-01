namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public interface IGameArgumentCollectionBuilder
{
    void Add(IGameArgument argument);

    bool Remove(IGameArgument argument);

    void AddAll(IGameArgumentCollection argumentCollection);

    IGameArgumentCollection Build();
}