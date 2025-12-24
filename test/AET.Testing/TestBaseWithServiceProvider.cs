// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.Extensions.DependencyInjection;
using System;

namespace AET.Testing;

/// <summary>
/// Provides a base class for test fixtures that provides and <see cref="IServiceProvider"/> for dependency injection.
/// </summary>
/// <remarks>Derive from this class to set up and access services using dependency injection in test scenarios.
/// Override <see cref="SetupServices(IServiceCollection)"/> to register custom services required for your
/// tests.</remarks>
public abstract class TestBaseWithServiceProvider
{
    /// <summary>
    /// Provides access to the application's service provider for resolving dependencies within derived classes.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider;

    /// <summary>
    /// Initializes a new instance of the class and configures the service provider.
    /// </summary>
    /// <remarks>
    /// This constructor creates a new service collection, invokes the <see cref="SetupServices"/> method to allow
    /// derived classes to register services, and then builds the service provider. Derived classes should override
    /// SetupServices to customize service registration.
    /// </remarks>
    protected TestBaseWithServiceProvider()
    {
        var sc = new ServiceCollection();
        // ReSharper disable once VirtualMemberCallInConstructor
        SetupServices(sc);
        ServiceProvider = sc.BuildServiceProvider();
    }

    /// <summary>
    /// Configures test services by adding them to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to which services will be added.</param>
    protected virtual void SetupServices(IServiceCollection serviceCollection)
    {
    }
}