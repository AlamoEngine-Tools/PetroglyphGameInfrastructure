using System;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Attribute to indicate that an enum shall be serialized by its underlying value.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class SerializeEnumValueAttribute : Attribute { }