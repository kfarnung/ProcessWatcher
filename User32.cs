using System;
using System.Runtime.InteropServices;

namespace ProcessWatcher
{
    internal static class User32
    {
        internal const int SW_NORMAL = 1;

        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
