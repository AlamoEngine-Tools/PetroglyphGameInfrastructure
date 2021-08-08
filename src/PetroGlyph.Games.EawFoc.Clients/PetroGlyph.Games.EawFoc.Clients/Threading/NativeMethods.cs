using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PetroGlyph.Games.EawFoc.Clients.Threading
{
    internal static class NativeMethods
    {
        internal const RegistryChangeNotificationFilters RegNotifyThreadAgnostic = (RegistryChangeNotificationFilters)0x10000000L;

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern int WaitForMultipleObjects(uint handleCount, IntPtr[] waitHandles, [MarshalAs(UnmanagedType.Bool)] bool waitAll, uint millisecondsTimeout);

        [DllImport("Advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern int RegNotifyChangeKeyValue(
            SafeRegistryHandle hKey,
            [MarshalAs(UnmanagedType.Bool)] bool watchSubtree,
            RegistryChangeNotificationFilters notifyFilter,
            SafeWaitHandle hEvent,
            [MarshalAs(UnmanagedType.Bool)] bool asynchronous);
    }
}