#if !NET5_0_OR_GREATER
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace System.Runtime.Versioning;

/// <summary>
/// Base type for all platform-specific API attributes.
/// </summary>

internal abstract class OSPlatformAttribute(string platformName) : Attribute
{
    public string PlatformName { get; } = platformName;
}

/// <summary>
/// Records the platform that the project targeted.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
internal sealed class TargetPlatformAttribute(string platformName) : OSPlatformAttribute(platformName);

/// <summary>
/// Records the operating system (and minimum version) that supports an API. Multiple attributes can be
/// applied to indicate support on multiple operating systems.
/// </summary>
/// <remarks>
/// Callers can apply a <see cref="SupportedOSPlatformAttribute " />
/// or use guards to prevent calls to APIs on unsupported operating systems.
///
/// A given platform should only be specified once.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly |
                AttributeTargets.Class |
                AttributeTargets.Constructor |
                AttributeTargets.Enum |
                AttributeTargets.Event |
                AttributeTargets.Field |
                AttributeTargets.Interface |
                AttributeTargets.Method |
                AttributeTargets.Module |
                AttributeTargets.Property |
                AttributeTargets.Struct,
    AllowMultiple = true, Inherited = false)]
internal sealed class SupportedOSPlatformAttribute(string platformName) : OSPlatformAttribute(platformName);
// ReSharper restore InconsistentNaming
#endif