﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PetroGlyph.Games.EawFoc.Clients.Steam.NativeMethods;

internal class WindowHandleInfo
{
    private readonly IntPtr _mainHandle;

    public WindowHandleInfo(IntPtr handle)
    {
        _mainHandle = handle;
    }

    public List<IntPtr> GetAllChildHandles()
    {
        List<IntPtr> childHandles = new();

        var gcChildHandlesList = GCHandle.Alloc(childHandles);
        var pointerChildHandlesList = GCHandle.ToIntPtr(gcChildHandlesList);

        try
        {
            User32.EnumWindowProc childProc = EnumWindow;
            User32.EnumChildWindows(_mainHandle, childProc, pointerChildHandlesList);
        }
        finally
        {
            gcChildHandlesList.Free();
        }
        return childHandles;
    }

    private static bool EnumWindow(IntPtr hWnd, IntPtr lParam)
    {
        var gcChildHandlesList = GCHandle.FromIntPtr(lParam);
        if (gcChildHandlesList.Target == null)
            return false;
        var childHandles = (List<IntPtr>)gcChildHandlesList.Target;
        childHandles.Add(hWnd);
        return true;
    }
}