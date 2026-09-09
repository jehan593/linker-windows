using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Linker.Theme
{
    public static class ImmersiveDarkMode
    {
        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaUseImmersiveDarkModeBefore20H1 = 19;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);

        public static void Apply(Window window, bool dark)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                window.SourceInitialized += (s, e) =>
                {
                    var h = new WindowInteropHelper(window).Handle;
                    ApplyToHandle(h, dark);
                };
                return;
            }
            ApplyToHandle(handle, dark);
        }

        private static void ApplyToHandle(IntPtr hwnd, bool dark)
        {
            if (hwnd == IntPtr.Zero) return;
            var value = dark ? 1 : 0;
            if (DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref value, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkModeBefore20H1, ref value, sizeof(int));
            }
        }
    }
}
