﻿namespace PetroGlyph.Games.EawFoc.Clients.Steam.Threading;

// From https://github.com/microsoft/vs-threading
internal readonly struct EmptyStruct
{
    internal static EmptyStruct Instance => default;
}