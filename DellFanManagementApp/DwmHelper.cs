using System;
using System.Runtime.InteropServices;

namespace DellFanManagement.App
{
    /// <summary>
    /// Helper class for applying DWM Acrylic / blur effects to windows.
    /// </summary>
    internal static class DwmHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic
        private const int DWMWCP_ROUND = 2;
        private const int DWM_COLOR_DEFAULT = unchecked((int)0xFFFFFFFF);

        [StructLayout(LayoutKind.Sequential)]
        internal struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        /// <summary>
        /// Enables an Acrylic / blur-behind effect on the specified window handle.
        /// Uses native DWM Acrylic on Windows 11 22H2+, falls back to blur-behind on older systems.
        /// </summary>
        /// <param name="hwnd">Window handle.</param>
        /// <param name="gradientColor">Color in 0xAARRGGBB format (fallback for older Windows).</param>
        public static void EnableAcrylic(IntPtr hwnd, int gradientColor)
        {
            // Windows 11 22H2+ (build 22621): Use native DWM Acrylic backdrop.
            // This is system-managed and does not cover window contents.
            if (Environment.OSVersion.Version.Build >= 22621)
            {
                // Windows 11: 使用 DWM 原生 Acrylic 背景
                int backdropType = DWMSBT_TRANSIENTWINDOW;
                DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));

                // 使用亮色模式，与白色窗体背景匹配
                int useDarkMode = 0;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));

                // 设置圆角窗口
                int cornerPreference = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

                // 将 DWM 玻璃边框扩展到客户区边缘，形成毛玻璃边框效果
                // 这样客户区边缘会显示出 Acrylic 毛玻璃质感
                var margins = new MARGINS
                {
                    cxLeftWidth = 8,
                    cxRightWidth = 8,
                    cyTopHeight = 8,
                    cyBottomHeight = 8
                };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
                return;
            }

            // Windows 10 / older Windows 11: Use SetWindowCompositionAttribute.
            // ACCENT_ENABLE_BLURBEHIND blurs content *behind* the window
            // without covering the window's own contents (unlike ACCENT_ENABLE_ACRYLICBLURBEHIND).
            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND,
                AccentFlags = 2,
                GradientColor = gradientColor,
                AnimationId = 0
            };

            int accentStructSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(hwnd, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }
    }
}
