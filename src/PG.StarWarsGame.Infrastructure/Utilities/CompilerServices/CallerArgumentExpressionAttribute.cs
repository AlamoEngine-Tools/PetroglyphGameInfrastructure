using System;

#if !NET
#pragma warning disable IDE0130
namespace PG.StarWarsGame.Infrastructure.Utilities.CompilerServices;
#pragma warning restore IDE0130

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
    public string ParameterName { get; } = parameterName;
}
#endif
