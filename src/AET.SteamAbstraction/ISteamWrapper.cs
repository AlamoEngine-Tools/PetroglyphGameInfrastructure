﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;

namespace AET.SteamAbstraction;

/// <summary>
/// .NET Wrapper to interact with the Steam Client
/// </summary>
public interface ISteamWrapper : IDisposable
{
    /// <summary>
    /// Returns <see langword="true"/> is Steam is installed on this machine; <see langword="false"/> otherwise.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
    bool Installed { get; }

    /// <summary>
    /// Returns <see langword="true"/> is Steam is currently running; <see langword="false"/> otherwise.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
    bool IsRunning { get; }

    /// <summary>
    /// Gets a value indicating whether a user is logged in. 
    /// </summary>
    /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
    bool IsUserLoggedIn { get; }

    /// <summary>
    /// Gets a value indicating whether the most recent user has activated the offline mode.
    /// <see langword="null"/> if Steam is not installed or the status could not be retrieved.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
    bool? UserWantsOfflineMode { get; }

    /// <summary>
    /// Gets an enumerable collection of Steam game libraries.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
    IEnumerable<ISteamLibrary> Libraries { get; }

    /// <summary>
    /// Checks whether the specified game is installed.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <param name="manifest">When this method returns, the manifest of the installed game, if the game is installed or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the game is installed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="SteamNotFoundException">Steam is not installed.</exception>
    /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
    bool IsGameInstalled(uint gameId, [NotNullWhen(true)] out SteamAppManifest? manifest);

    /// <summary>
    /// Starts the Steam client.
    /// </summary>
    /// <exception cref="SteamNotFoundException">Steam is not installed.</exception>
    /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
    void StartSteam();

    /// <summary>
    /// Returns a Task that completes when the Steam client is started and a user is logged in.
    /// <para>
    /// If the user requested Offline mode, the returned task completes when the Steam client is ready to use.
    /// </para>
    /// </summary>
    /// <param name="startIfNotRunning">When set to <see langword="true"/> Steam will be started, if not already running.
    /// When set to <see langword="false"/> Steam must be started manually.</param>
    /// <param name="cancellation">A token that may be canceled to release the resources from waiting
    /// for Steam and complete the returned Task as canceled.</param>
    /// <returns>
    /// A task that completes when Steam is running and the user is logged in, or upon cancellation.
    /// </returns>
    /// <exception cref="SteamNotFoundException">Steam is not installed.</exception>
    /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
    /// <exception cref="SteamException">Steam is not running.</exception>
    Task WaitSteamRunningAndLoggedInAsync(bool startIfNotRunning, CancellationToken cancellation = default);
}