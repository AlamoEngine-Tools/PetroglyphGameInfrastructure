using System;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;

/// <summary>
/// Attribute to indicate that an enum shall be serialized by its underlying value.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
internal class SerializeEnumValueAttribute : Attribute;