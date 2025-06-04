using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SnipToPdf
{
    internal static class HotkeyManager
    {
        // -------- constants ----------
        internal const int WM_HOTKEY = 0x0312;

        internal const uint MOD_ALT     = 0x0001;
        internal const uint MOD_CONTROL = 0x0002;
        internal const uint MOD_SHIFT   = 0x0004;
        internal const uint MOD_WIN     = 0x0008;

        // -------- native P/Invoke ----
        //  give the native symbols unique *managed* names
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterHotKey")]
        private static extern bool RegisterHotKeyNative(
            IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "UnregisterHotKey")]
        private static extern bool UnregisterHotKeyNative(
            IntPtr hWnd, int id);

        // -------- public wrappers ----
        internal static bool RegisterHotKey(
            IntPtr windowHandle, int id, uint modifiers, Keys key) =>
            RegisterHotKeyNative(windowHandle, id, modifiers, (uint)key);

        internal static bool UnregisterHotKey(
            IntPtr windowHandle, int id) =>
            UnregisterHotKeyNative(windowHandle, id);
    }
}
