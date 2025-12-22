using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;

namespace AET.Testing;

/// <summary>
/// A <see cref="TestBaseWithServiceProvider"/> test base that automatically registers an <see cref="IFileSystem"/> as service.
/// </summary>
public abstract class TestBaseWithFileSystem : TestBaseWithServiceProvider
{
    /// <summary>
    /// Gets the file system abstraction used for testing purposes.
    /// This property provides access to an <see cref="IFileSystem"/> instance, which is lazily initialized
    /// and can be overridden by derived classes to customize the file system behavior.
    /// </summary>
    /// <remarks>
    /// The file system is initialized using the <see cref="CreateFileSystem"/> method. If the initialization
    /// fails or returns <c>null</c>, an <see cref="InvalidOperationException"/> is thrown.
    /// </remarks>
    [field: MaybeNull, AllowNull]
    protected IFileSystem FileSystem => LazyInitializer.EnsureInitialized(ref field, CreateFileSystem)
                                        ?? throw new InvalidOperationException("Creation of file system must not return null.");

    /// <summary>
    /// Initializes a new instance of the class and configures the service provider.
    /// </summary>
    /// <remarks>
    /// This constructor creates a new service collection, invokes the <see cref="SetupServices"/> method to allow
    /// derived classes to register services, and then builds the service provider. Derived classes should override
    /// SetupServices to customize service registration.
    /// </remarks>
    protected TestBaseWithFileSystem()
    {
    }

    /// <summary>
    /// Creates and returns a new instance of the file system abstraction for testing purposes.
    /// </summary>
    /// <remarks>
    /// This method is invoked to initialize the <see cref="FileSystem"/> property. By default, it returns
    /// a <see cref="MockFileSystem"/> instance, but derived classes can override this method to provide
    /// a custom implementation of <see cref="IFileSystem"/>.
    /// </remarks>
    /// <returns>
    /// An instance of <see cref="IFileSystem"/> representing the file system abstraction to be used in tests.
    /// </returns>
    protected virtual IFileSystem CreateFileSystem()
    {
        return new MockFileSystem();
    }


    /// <inheritdoc />
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton(FileSystem);
    }
}