// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace AET.SteamAbstraction.Testing;
internal sealed partial class TestingSteamInstallationImpl
{
    public void DeleteLoginUsersFile()
    {
        var configPath =
            _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(TestingSteamConstants.SteamInstallPath, "config"));
        var loginUsersPath = _fileSystem.Path.Combine(configPath, "loginusers.vdf");
        _fileSystem.File.Delete(loginUsersPath);
    }

    public void WriteCorruptLoginUsers()
    {
        var configPath =
            _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(TestingSteamConstants.SteamInstallPath, "config"));
        var loginUsersPath = _fileSystem.Path.Combine(configPath, "loginusers.vdf");
        _fileSystem.File.WriteAllText(loginUsersPath, "\0");
    }

    public IFileInfo WriteLoginUsers(params IEnumerable<TestingSteamUserLoginMetadata>? users)
    {
        var configPath =
            _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(TestingSteamConstants.SteamInstallPath, "config"));
        var loginUsersPath = _fileSystem.Path.Combine(configPath, "loginusers.vdf");

        var content = $@"
""users""
{{
    {SerializeUsers(users?.Select(x => new SteamUserLoginMetadata(x.UserId, x.MostRecent, x.UserWantsOffline)))}
}}
";
        _fileSystem.File.WriteAllText(loginUsersPath, content);
        return _fileSystem.FileInfo.New(loginUsersPath);
    }

    private static string SerializeUsers(IEnumerable<SteamUserLoginMetadata>? users)
    {
        var sb = new StringBuilder();

        if (users is null)
            return string.Empty;

        foreach (var metadata in users)
        {
            var content = $@"
    ""{metadata.UserId}""
    {{
	    ""AccountName""		""someName""
	    ""PersonaName""		""some Name""
	    ""RememberPassword""		""1""
	    ""WantsOfflineMode""		""{BoolToNumber(metadata.UserWantsOffline)}""
	    ""SkipOfflineModeWarning""		""0""
	    ""AllowAutoLogin""		""1""
	    ""MostRecent""		""{BoolToNumber(metadata.MostRecent)}""
	    ""Timestamp""		""0000000000""
    }}";

            sb.AppendLine(content);
        }

        return sb.ToString();
    }


    private static int BoolToNumber(bool value)
    {
        return value ? 1 : 0;
    }
}