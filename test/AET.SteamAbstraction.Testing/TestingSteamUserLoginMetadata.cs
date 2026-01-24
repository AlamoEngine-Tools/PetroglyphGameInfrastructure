// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace AET.SteamAbstraction.Testing;

/// <summary>
/// Represent metadata about a Steam user login.
/// </summary>
public sealed class TestingSteamUserLoginMetadata
{
    /// <summary>
    /// Gets or sets the ID of the login user.
    /// </summary>
    public ulong UserId { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the user wants to use Steam in offline mode.
    /// </summary>
    public bool UserWantsOffline { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this login is the most recent one.
    /// </summary>
    public bool MostRecent { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestingSteamUserLoginMetadata"/> class.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="mostRecent">The information whether this user represents the most recent login.</param>
    /// <param name="userWantsOffline">The information whether this user wants to stay offline.</param>
    public TestingSteamUserLoginMetadata(ulong userId, bool mostRecent = false, bool userWantsOffline = false)
    {
        UserId = userId;
        MostRecent = mostRecent;
        UserWantsOffline = userWantsOffline;
    }
}