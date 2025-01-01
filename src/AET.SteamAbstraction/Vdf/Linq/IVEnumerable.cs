using System.Collections.Generic;

namespace AET.SteamAbstraction.Vdf.Linq;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal interface IVEnumerable<out T> : IEnumerable<T> where T : VToken
{
    IVEnumerable<VToken> this[object key] { get; }
}