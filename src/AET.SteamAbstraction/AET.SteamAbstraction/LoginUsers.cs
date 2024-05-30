using System;
using System.Collections.Generic;

namespace AET.SteamAbstraction;

internal class LoginUsers(IEnumerable<SteamUserLoginMetadata> users)
{
    public IEnumerable<SteamUserLoginMetadata> Users { get; } = users ?? throw new ArgumentNullException(nameof(users));
}