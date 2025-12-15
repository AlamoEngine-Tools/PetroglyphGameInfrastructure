using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace AET.SteamAbstraction.Testing;

public interface ITestingSteamInstallation : IDisposable
{
    IDirectoryInfo? InstallationDirectory { get; }

    ITestingSteamRegistry Registry { get; }

    void Install();

    void InstallSteamFilesOnly();
    
    ISteamFakeProcess FakeStart(int pid);

    ITestingSteamLibrary InstallDefaultLibrary(bool addToConfig = true);

    ITestingSteamLibrary InstallLibrary(string path, bool addToConfig = true);

    void WriteCorruptLoginUsers();

    void DeleteLoginUsersFile();
    
    IFileInfo WriteLoginUsers(params IEnumerable<TestingSteamUserLoginMetadata>? users);
}