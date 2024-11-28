using System;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

/// <summary>
/// 
/// </summary>
public class ModDependencyException : ModException
{
    /// <summary>
    /// 
    /// </summary>
    public IModReference Dependency { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    public ModDependencyException(IModReference source, IModReference dependency) : base(source)
    {
        Dependency = dependency;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    /// <param name="message"></param>
    public ModDependencyException(IModReference source, IModReference dependency, string message) 
        : base(source, message)
    {
        Dependency = dependency;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public ModDependencyException(IModReference source, IModReference dependency, string message, Exception exception) 
        : base(source, message, exception)
    {
        Dependency = dependency;
    }
}