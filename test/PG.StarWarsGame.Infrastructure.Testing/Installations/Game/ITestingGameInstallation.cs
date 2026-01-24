// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;
using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

/// <summary>
/// Represents an abstraction for a test installation game installation, providing methods and properties to install mods.
/// </summary>
public interface ITestingGameInstallation : ITestingModContainerInstallation, ITestingPhysicalPlayableObjectInstallation
{
    /// <summary>
    /// Gets the game instance associated with this installation.
    /// </summary>
    IGame Game { get; }
    
    /// <summary>
    /// Installs debug executable files to the game installation.
    /// </summary>
    void InstallDebug();
    
    /// <summary>
    /// Retrieves the incorrect directory from the EA Origin Forces of Corruption setup that gets written to the registry.
    /// </summary>
    /// <returns>The directory information of the incorrect registry location.</returns>
    IDirectoryInfo GetWrongOriginFocRegistryLocation();
    
    /// <summary>
    /// Installs a mod with the specified name.
    /// </summary>
    /// <remarks>
    /// Depending on the platform of the game installation, the installed mod is randomly selected to be a normal or Workshops mod.
    /// </remarks>
    /// <param name="name">The name of the mod to install.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed mod.</returns>
    ITestingPhysicalModInstallation InstallMod(string name);

    /// <summary>
    /// Installs a mod with the specified name and workshop flag.
    /// </summary>
    /// <param name="name">The name of the mod to install.</param>
    /// <param name="workshop">Indicates whether the mod is a Workshops mod.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed mod.</returns>
    ITestingPhysicalModInstallation InstallMod(string name, bool workshop);

    /// <summary>
    /// Installs a mod using the provided mod information.
    /// </summary>
    /// <remarks>
    /// Depending on the platform of the game installation, the installed mod is randomly selected to be a normal or Workshops mod.
    /// </remarks>
    /// <param name="modinfo">The mod information to use for installation.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed mod.</returns>
    ITestingPhysicalModInstallation InstallMod(IModinfo modinfo);
    
    /// <summary>
    /// Installs a mod using the provided mod information and workshop flag.
    /// </summary>
    /// <param name="modinfo">The mod information to use for installation.</param>
    /// <param name="workshop">Indicates whether the mod is a Workshops mod.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed mod.</returns>
    ITestingPhysicalModInstallation InstallMod(IModinfo modinfo, bool workshop);
    
    /// <summary>
    /// Installs a mod using the provided mod information, directory, and workshop flag.
    /// </summary>
    /// <param name="modinfo">The mod information to use for installation.</param>
    /// <param name="directory">The directory where the mod will be installed.</param>
    /// <param name="workshop">Indicates whether the mod is a workshop mod.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed mod.</returns>
    ITestingPhysicalModInstallation InstallMod(IModinfo modinfo, IDirectoryInfo directory, bool workshop);

    /// <summary>
    /// Installs and adds a mod with the specified name to the game installation.
    /// </summary>
    /// <remarks>
    /// Depending on the platform of the game installation, the installed mod is randomly selected to be a normal or Workshops mod.
    /// </remarks>
    /// <param name="name">The name of the mod to install and add.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed and added mod.</returns>
    ITestingPhysicalModInstallation InstallAndAddMod(string name);

    /// <summary>
    /// Installs and adds a mod with the specified name and workshop flag to the game installation.
    /// </summary>
    /// <param name="name">The name of the mod to install and add.</param>
    /// <param name="workshop">Indicates whether the mod is a Workshops mod.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed and added mod.</returns>
    ITestingPhysicalModInstallation InstallAndAddMod(string name, bool workshop);

    /// <summary>
    /// Installs and adds a mod using the provided mod information to the game installation.
    /// </summary>
    /// <remarks>
    /// Depending on the platform of the game installation, the installed mod is randomly selected to be a normal or Workshops mod.
    /// </remarks>
    /// <param name="modinfo">The mod information to use for installation and addition.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed and added mod.</returns>
    ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo);

    /// <summary>
    /// Installs and adds a mod using the provided mod information and workshop flag to the game installation.
    /// </summary>
    /// <param name="modinfo">The mod information to use for installation and addition.</param>
    /// <param name="workshop">Indicates whether the mod is a Workshops mod.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed and added mod.</returns>
    ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo, bool workshop);

    /// <summary>
    /// Installs and adds a mod using the provided mod information, directory, and workshop flag to the game installation.
    /// </summary>
    /// <param name="modinfo">The mod information to use for installation and addition.</param>
    /// <param name="directory">The directory where the mod will be installed and added.</param>
    /// <param name="workshops">Indicates whether the mod is a Workshops mod.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed and added mod.</returns>
    ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo, IDirectoryInfo directory, bool workshops);

    /// <summary>
    /// Installs and adds a mod with the specified name, workshop flag, and dependencies to the game installation.
    /// </summary>
    /// <param name="name">The name of the mod to install and add.</param>
    /// <param name="isWorkshop">Indicates whether the mod is a Workshops mod.</param>
    /// <param name="dependencies">The dependencies of the mod.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed and added mod.</returns>
    ITestingPhysicalModInstallation InstallAndAddMod(string name, bool isWorkshop, IModDependencyList dependencies);

    /// <summary>
    /// Installs and adds a mod with the specified name and dependencies to the game installation.
    /// </summary>
    /// <remarks>
    /// Depending on the platform of the game installation, the installed mod is randomly selected to be a normal or Workshops mod.
    /// </remarks>
    /// <param name="name">The name of the mod to install and add.</param>
    /// <param name="dependencies">The dependencies of the mod.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed and added mod.</returns>
    ITestingPhysicalModInstallation InstallAndAddMod(string name, IModDependencyList dependencies);

    /// <summary>
    /// Adds a virtual mod with the specified name and mod information to the game installation.
    /// </summary>
    /// <param name="name">The name of the virtual mod to add.</param>
    /// <param name="modinfo">The mod information of the virtual mod.</param>
    /// <returns>An instance of <see cref="ITestingVirtualModInstallation"/> representing the added virtual mod.</returns>
    ITestingVirtualModInstallation AddVirtualMod(string name, ModinfoData modinfo);

    /// <summary>
    /// Gets the directory of a mod for the specified name and workshop flag.
    /// </summary>
    /// <remarks>The created <see cref="IDirectoryInfo"/> is not created on the file system.</remarks>
    /// <param name="name">The name of the mod.</param>
    /// <param name="workshop">Indicates whether the mod is a workshop mod.</param>
    /// <returns>The directory information of the mod.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="workshop"/> is <see langword="true"/>
    /// but the associated game is not a Steam installation.
    /// </exception>
    IDirectoryInfo GetModDirectory(string name, bool workshop);
}