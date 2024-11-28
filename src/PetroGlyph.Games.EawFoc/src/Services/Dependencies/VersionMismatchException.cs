using System;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

/// <summary>
/// 
/// </summary>
public class VersionMismatchException : ModDependencyException
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    public VersionMismatchException(IModReference source, IModReference dependency) : base(source, dependency)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    /// <param name="message"></param>
    public VersionMismatchException(IModReference source, IModReference dependency, string message) : base(source, dependency, message)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public VersionMismatchException(IModReference source, IModReference dependency, string message, Exception exception) : base(source, dependency, message, exception)
    {
    }
}