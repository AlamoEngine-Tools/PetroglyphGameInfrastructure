using System;

namespace AET.Testing.Attributes;

[Flags]
public enum TestPlatformIdentifier
{
    Windows = 1,
    Linux = 2,
}