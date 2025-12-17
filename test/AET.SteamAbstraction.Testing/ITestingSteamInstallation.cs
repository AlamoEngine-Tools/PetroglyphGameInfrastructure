using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.Versioning;

namespace AET.SteamAbstraction.Testing;

public interface ITestingSteamInstallation : IDisposable
{
    IDirectoryInfo? InstallationDirectory { get; }

    ITestingSteamRegistry Registry { get; }

    [SupportedOSPlatform("windows")]
    void Install();

    void InstallSteamFilesOnly();

    [SupportedOSPlatform("windows")]
    ISteamFakeProcess FakeStart(int pid);

    ITestingSteamLibrary InstallDefaultLibrary(bool addToConfig = true);

    ITestingSteamLibrary InstallLibrary(string path, bool addToConfig = true);

    void WriteCorruptLoginUsers();

    void DeleteLoginUsersFile();
    
    IFileInfo WriteLoginUsers(params IEnumerable<TestingSteamUserLoginMetadata>? users);
}