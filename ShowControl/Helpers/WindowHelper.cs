using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ShowControl.Helpers
{
    /// <summary>
    /// Helper class for Windows-specific window customization
    /// </summary>
    public static class WindowHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        /// <summary>
        /// Enables dark mode for the window title bar on Windows 10/11
        /// </summary>
        /// <param name="window">The window to apply dark mode to</param>
        public static void EnableDarkTitleBar(Window window)
        {
            if (window == null) return;

            var windowHandle = new WindowInteropHelper(window).Handle;
            if (windowHandle == IntPtr.Zero) return;

            // Try the newer attribute first (Windows 10 build 22000+)
            var darkMode = 1;
            int result = DwmSetWindowAttribute(windowHandle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            
            // If that fails, try the older attribute (Windows 10 build 17763-21996)
            if (result != 0)
            {
                DwmSetWindowAttribute(windowHandle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref darkMode, sizeof(int));
            }
        }

        /// <summary>
        /// Sets a custom accent color for the window title bar (Windows 11)
        /// </summary>
        /// <param name="window">The window to apply the color to</param>
        /// <param name="color">The color to use (ARGB format)</param>
        public static void SetTitleBarColor(Window window, uint color)
        {
            if (window == null) return;

            var windowHandle = new WindowInteropHelper(window).Handle;
            if (windowHandle == IntPtr.Zero) return;

            const int DWMWA_CAPTION_COLOR = 35;
            int colorValue = (int)color;
            DwmSetWindowAttribute(windowHandle, DWMWA_CAPTION_COLOR, ref colorValue, sizeof(int));
        }
    }
}