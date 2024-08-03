using System;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Attribute to indicate that an enum shall be serialized by its name.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
internal class SerializeEnumNameAttribute : Attribute;