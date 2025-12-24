// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace AET.SteamAbstraction.Testing;

/// <summary>
/// Provides factory methods for creating abstractions of Steam installations for testing purposes.
/// </summary>
/// <remarks>
/// This class is intended for use in testing scenarios where mock or test implementations of Steam-related services are required.
/// </remarks>
public static class SteamTesting
{
    /// <summary>
    /// Creates a new instance of a Steam installation abstraction for testing using the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies required by the testing Steam installation.</param>
    /// <returns>An instance of <see cref="ITestingSteamInstallation"/> initialized with the provided service provider.</returns>
    public static ITestingSteamInstallation Steam(IServiceProvider serviceProvider)
    {
        return new TestingSteamInstallationImpl(serviceProvider);
    }

    /// <summary>
    /// Creates a new instance of a Steam registry abstraction for testing using the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies required by the testing Steam registry implementation.</param>
    /// <returns>An instance of <see cref="ITestingSteamRegistry"/> initialized with the provided service provider.</returns>
    public static ITestingSteamRegistry SteamRegistry(IServiceProvider serviceProvider)
    {
        return new TestingSteamRegistryImpl(serviceProvider);
    }
}