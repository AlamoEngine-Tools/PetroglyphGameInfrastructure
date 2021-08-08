using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Process
{
    public interface IProcessCreator
    {
        System.Diagnostics.Process StartProcess(IFileInfo exeFile, string arguments);
    }
}