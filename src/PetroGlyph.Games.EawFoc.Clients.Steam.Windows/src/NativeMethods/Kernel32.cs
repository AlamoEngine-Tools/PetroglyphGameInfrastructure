using System;
using System.Runtime.InteropServices;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam.NativeMethods;

internal static class Kernel32
{
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    internal static extern int WaitForMultipleObjects(uint handleCount, IntPtr[] waitHandles, [MarshalAs(UnmanagedType.Bool)] bool waitAll, uint millisecondsTimeout);
}