using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace DoomRemake.Core
{
    /// <summary>
    /// Windows Input Hooks for capturing keyboard and mouse state.
    /// Uses User32.dll API calls.
    </summary>
    public static class InputHooks
    {
        // Keyboard hook structures
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Mouse hook structures
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Hook procedure delegate signatures
        private delegate int LowLevelKeyboardProc(ref KBDLLHOOKSTRUCT kb);
        private delegate int LowLevelMouseProc(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT ms);

        private static LowLevelKeyboardProc? _keyboardHook;
        private static LowLevelMouseProc? _mouseHook;
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize Windows Input hooks to capture keyboard and mouse state.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                // Set up keyboard hook
                _keyboardHook = LowLevelKeyboardProcWrapper;
                uint result = SetWindowsHookEx(0x02, _keyboardHook, GetModuleHandle(IntPtr.Zero), 0);
                
                if (result == IntPtr.Zero)
                {
                    throw new Exception("Failed to set up keyboard hook");
                }

                // Set up mouse hook
                _mouseHook = LowLevelMouseProcWrapper;
                result = SetWindowsHookEx(0x03, _mouseHook, GetModuleHandle(IntPtr.Zero), 0);
                
                if (result == IntPtr.Zero)
                {
                    throw new Exception("Failed to set up mouse hook");
                }

                _isInitialized = true;
                Console.WriteLine("[INFO] Input hooks are active and capturing events.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Could not initialize input hooks: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Clean up and remove all input hooks.
        /// </summary>
        public static void Shutdown()
        {
            if (_keyboardHook != null)
            {
                UnhookWindowsHookEx(_keyboardHook);
                _keyboardHook = null;
            }

            if (_mouseHook != null)
            {
                UnhookWindowsHookEx(_mouseHook);
                _mouseHook = null;
            }

            _isInitialized = false;
        }

        private static int LowLevelKeyboardProcWrapper(ref KBDLLHOOKSTRUCT kb)
        {
            // Handle key events here
            if (kb.flags == 0x40) // WM_KEYDOWN
            {
                Console.WriteLine($"Key pressed: {(char)kb.vkCode}");
            }
            else if (kb.flags == 0x10) // WM_KEYUP
            {
                Console.WriteLine($"Key released: {(char)kb.vkCode}");
            }

            return CallNextHookEx(IntPtr.Zero, 0, IntPtr.Zero, ref kb);
        }

        private static int LowLevelMouseProcWrapper(int nCode, IntPtr wParam, ref MSLLHOOKSTRUCT ms)
        {
            // Handle mouse events here
            if (nCode >= 0)
            {
                switch ((uint)wParam)
                {
                    case 0x02: // WM_LBUTTONDOWN
                        Console.WriteLine($"Left Mouse Button Down at ({ms.pt.X}, {ms.pt.Y})");
                        break;
                    case 0x03: // WM_RBUTTONDOWN
                        Console.WriteLine($"Right Mouse Button Down at ({ms.pt.X}, {ms.pt.Y})");
                        break;
                    case 0x200: // WM_MOUSEWHEEL
                        Console.WriteLine($"Mouse Wheel scrolled by {(short)(GetWheelDelta(wParam))} degrees");
                        break;
                }
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, ref ms);
        }

        private static short GetWheelDelta(IntPtr wParam)
        {
            int result = Marshal.ReadInt32((IntPtr)(wParam.ToInt64() + 8));
            return (short)((result >> 16) & 0xFFFF);
        }
    }

    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}