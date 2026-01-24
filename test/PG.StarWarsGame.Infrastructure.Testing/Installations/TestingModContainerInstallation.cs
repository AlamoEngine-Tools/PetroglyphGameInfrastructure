// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

internal abstract class TestingModContainerInstallation(IServiceProvider serviceProvider)
    : TestingPlayableObjectInstallationImpl(serviceProvider), ITestingModContainerInstallation
{
    public abstract PlayableModContainer ModContainer { get; }
}