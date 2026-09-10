using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Linker.Util
{
    public static class WindowTransitions
    {
        private const int DwmwaTransitionsForcedDisabled = 3;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);

        public static void DisableAnimations(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                window.SourceInitialized += (s, e) => Apply(new WindowInteropHelper(window).Handle);
                return;
            }
            Apply(handle);
        }

        private static void Apply(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;
            var value = 1;
            DwmSetWindowAttribute(hwnd, DwmwaTransitionsForcedDisabled, ref value, sizeof(int));
        }
    }
}