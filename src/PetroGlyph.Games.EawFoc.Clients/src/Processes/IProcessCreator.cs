using System.Diagnostics;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

public interface IProcessCreator
{
    Process StartProcess(IFileInfo exeFile, string arguments);
}