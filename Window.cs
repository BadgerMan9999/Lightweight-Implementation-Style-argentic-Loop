// ============================================================================
// Window.cs - [LISL_W]
// Win32-based windowing system for Vulkan rendering
// ============================================================================

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace LISL_Core.PAL
{
    /// <summary>
    /// Win32 window class with message loop and Vulkan compatibility.
    /// </summary>
    public sealed class Window : IDisposable
    {
        private static readonly Lazy<Window> _instance = new(() => new Window());
        
        private IntPtr _hwnd;
        private bool _disposed;

        // Win32 constants
        private const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
        private const int CS_HREDRAW = 0x0001;
        private const int CS_VREDRAW = 0x0002;
        private const int WM_DESTROY = 0x0002;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_CHAR = 0x0102;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MOUSEWHEEL = 0x020A;

        // VK constants for key codes
        private static readonly int[] VK_CODES = new int[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, // 0-9
            0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, // A-F
            0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, // G-O
            0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, // P-Z
            0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, // [ and ]
            0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, // \ and '
            0x3C, 0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, // Enter and
            0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, // Left Ctrl and
            0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, // Left Shift and
            0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F, 0x60, 0x61, 0x62, 0x63, // Left Alt and
            0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, // Space and
            0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, // Caps Lock and
            0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F, 0x80, 0x81, // F1-F24
            0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, // Num Lock and
            0x8C, 0x8D, 0x8E, 0x8F, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, // Scroll Lock and
            0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F, // Home and
            0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, // Arrow keys
            0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF, 0xB0, 0xB1, 0xB2, 0xB3, // Insert and
            0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, // Delete and
            0xBE, 0xBF, 0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, // Page Up and
            0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF, 0xD0, 0xD1, // Page Down and
            0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, // End and
            0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, // Select and
            0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, // Print and
            0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, // Execute and
            0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF // Help and
        };

        public static Window Instance => _instance.Value;

        private Window()
        {
            var result = CreateWindowEx(
                0,
                "STATIC",
                "LISL Vulkan Doom Port",
                WS_OVERLAPPEDWINDOW & ~WS_CAPTION & ~WS_THICKFRAME & ~WS_MAXIMIZEBOX, // Borderless but resizable
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                1280,
                720,
                IntPtr.Zero,
                IntPtr.Zero,
                GetModuleHandle(null),
                IntPtr.Zero);

            if (result == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                Logger.Instance.Fatal($"Failed to create window: Error {error}");
            }

            _hwnd = result;
            
            // Start message loop in background thread
            MessageLoopThread.Start();
        }

        /// <summary>
        /// Get the HWND of this window.
        /// </summary>
        public IntPtr Handle => _hwnd;

        /// <summary>
        /// Check if the window is visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                var show = ShowWindow(_hwnd, 0);
                return (int)show == 1; // SW_SHOW
            }
        }

        /// <summary>
        /// Set window visibility.
        /// </summary>
        public void Show(bool visible) => ShowWindow(_hwnd, visible ? 9 : 0); // SW_SHOW or SW_HIDE

        /// <summary>
        /// Get the current window position and size.
        /// </summary>
        public (int x, int y, int width, int height) GetPositionAndSize()
        {
            var rect = new RECT();
            GetWindowRect(_hwnd, ref rect);
            return (rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        /// <summary>
        /// Set window position and size.
        /// </summary>
        public void SetPositionAndSize(int x, int y, int width, int height)
        {
            MoveWindow(_hwnd, x, y, width, height, true);
        }

        /// <summary>
        /// Close the window.
        /// </summary>
        public void Close() => PostQuitMessage(0);

        /// <summary>
        /// Message loop thread that processes Windows messages.
        /// </summary>
        private static class MessageLoopThread
        {
            private static readonly Thread _thread = new(ThreadMessageLoop) { IsBackground = true };
            
            public static void Start() => _thread.Start();

            private static void ThreadMessageLoop()
            {
                MSG msg;
                
                while (GetMessage(out msg, IntPtr.Zero, 0, 0))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }

            /// <summary>
            /// Process a Windows message.
            /// </summary>
            public static void ProcessMessage(MSG msg)
            {
                if (msg.message == WM_KEYDOWN || msg.message == WM_SYSKEYDOWN)
                {
                    var vk = VK_CODES[msg.wParam];
                    Logger.Instance.Info($"Key pressed: 0x{vk:X2}");
                }

                if (msg.message == WM_MOUSEMOVE)
                {
                    var pos = new POINT();
                    GetCursorPos(ref pos);
                    Logger.Instance.Info($"Mouse position: ({pos.x}, {pos.y})");
                }
            }
        }

        /// <summary>
        /// Win32 P/Invoke declarations.
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr CreateWindowEx(
                uint dwExStyle,
                string lpClassName,
                string lpWindowName,
                uint dwStyle,
                int X,
                int Y,
                int nWidth,
                int nHeight,
                IntPtr hWndParent,
                IntPtr hMenu,
                IntPtr hInstance,
                IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern bool DestroyWindow(IntPtr hwnd);

            [DllImport("user32.dll")]
            public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            public static extern bool MoveWindow(
                IntPtr hWnd,
                int X,
                int Y,
                int nWidth,
                int nHeight,
                bool bRepaint);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

            [DllImport("user32.dll")]
            public static extern IntPtr PostQuitMessage(int wExitCode);

            [DllImport("user32.dll")]
            public static extern bool GetMessage(
                ref MSG lpMsg,
                IntPtr hWnd,
                uint wMsgFilterMin,
                uint wMsgFilterMax);

            [DllImport("user32.dll")]
            public static extern bool TranslateMessage(ref MSG lpMsg);

            [DllImport("user32.dll")]
            public static extern bool DispatchMessage(ref MSG lpMsg);

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }

            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(ref POINT lpPoint);

            [StructLayout(LayoutKind.Sequential)]
            public struct MSG
            {
                public IntPtr hwnd;
                public uint message;
                public WPARAM wParam;
                public LPARAM lParam;
                public uint time;
                public POINT pt;
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [StructLayout(LayoutKind.Sequential)]
            public struct WPARAM { public int low; }

            [StructLayout(LayoutKind.Sequential)]
            public struct LPARAM { public int low; }
        }

        private Window() => throw new NotImplementedException(); // Prevent instantiation

        ~Window() => Dispose();
    }

    /// <summary>
    /// Input Manager for Win32 keyboard and mouse state.
    /// </summary>
    public sealed class InputManager : IDisposable
    {
        private static readonly Lazy<InputManager> _instance = new(() => new InputManager());
        
        private bool _disposed;

        // Keyboard state array (256 keys)
        private static readonly byte[] _keyboardState = new byte[256];
        private static readonly object _lockObject = new object();

        public static InputManager Instance => _instance.Value;

        private InputManager() { }

        /// <summary>
        /// Check if a key is currently pressed.
        /// </summary>
        public bool IsKeyDown(int virtualKey)
        {
            lock (_lockObject)
            {
                return _keyboardState[virtualKey] != 0;
            }
        }

        /// <summary>
        /// Check if a key was just pressed (edge detection).
        /// </summary>
        public bool WasKeyPressed(int virtualKey)
        {
            lock (_lockObject)
            {
                var wasPressed = _keyboardState[virtualKey] != 0;
                _keyboardState[virtualKey] = 0; // Clear state
                return wasPressed;
            }
        }

        /// <summary>
        /// Get the current mouse position.
        /// </summary>
        public (int x, int y) GetMousePosition()
        {
            var pos = new System.Drawing.Point();
            System.Windows.Forms.Cursor.Position = pos;
            return (pos.X, pos.Y);
        }

        /// <summary>
        /// Poll input state from Win32 messages.
        /// Should be called during the message loop.
        /// </summary>
        public static void PollInput()
        {
            MSG msg;
            
            while (WindowNativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                if (msg.message == WindowNativeMethods.WM_KEYDOWN || 
                    msg.message == WindowNativeMethods.WM_SYSKEYDOWN)
                {
                    var vk = WindowNativeMethodsWithVK(msg.wParam);
                    lock (_lockObject)
                    {
                        _keyboardState[vk] = 1; // Mark as pressed
                    }
                }

                if (msg.message == WindowNativeMethods.WM_CHAR)
                {
                    Logger.Instance.Info($"Character: '{(char)msg.wParam.low}'");
                }
            }
        }

        /// <summary>
        /// Convert Win32 wParam to virtual key code.
        /// </summary>
        private static int WindowNativeMethodsWithVK(WPARAM wParam) => 
            (int)(wParam.low & 0xFF);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }

        ~InputManager() => Dispose();
    }

    /// <summary>
    /// High-resolution time class using QueryPerformanceCounter.
    /// </summary>
    public sealed class Time : IDisposable
    {
        private static readonly Lazy<Time> _instance = new(() => new Time());
        
        private bool _disposed;

        public static Time Instance => _instance.Value;

        private Time() { }

        /// <summary>
        /// Get the current high-resolution time in ticks.
        /// </summary>
        public long GetCurrentTicks() => QueryPerformanceCounter();

        /// <summary>
        /// Get elapsed time since a previous tick in seconds.
        /// </summary>
        public double GetElapsedSeconds(long startTicks)
        {
            var now = QueryPerformanceCounter();
            return (double)(now - startTicks) / Frequency;
        }

        /// <summary>
        /// Get elapsed time since a previous tick in milliseconds.
        /// </summary>
        public double GetElapsedMilliseconds(long startTicks)
        {
            var now = QueryPerformanceCounter();
            return (double)(now - startTicks) / Frequency * 1000;
        }

        /// <summary>
        /// Get the frequency of the performance counter.
        /// </summary>
        public static long Frequency => QueryPerformanceFrequency(out _);

        private static void QueryPerformanceCounter() => 
            System.Diagnostics.QueryPerformanceCounter(out var ticks);

        private static bool QueryPerformanceFrequency(out long freq) => 
            System.Diagnostics.QueryPerformanceFrequency(out freq);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }

        ~Time() => Dispose();
    }

    // Helper class for Win32 interop
    internal static class WindowNativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int X,
            int Y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(
            IntPtr hWnd,
            int X,
            int Y,
            int nWidth,
            int nHeight,
            bool bRepaint);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        public static extern IntPtr PostQuitMessage(int wExitCode);

        [DllImport("user32.dll")]
        public static extern bool GetMessage(
            ref MSG lpMsg,
            IntPtr hWnd,
            uint wMsgFilterMin,
            uint wMsgFilterMax);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern bool DispatchMessage(ref MSG lpMsg);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public WPARAM wParam;
            public LPARAM lParam;
            public uint time;
            public POINT pt;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        public struct WPARAM { public int low; }

        [StructLayout(LayoutKind.Sequential)]
        public struct LPARAM { public int low; }

        // Message constants
        public const uint WM_DESTROY = 0x0002;
        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_SYSKEYDOWN = 0x0104;
        public const uint WM_CHAR = 0x0102;
        public const uint WM_MOUSEMOVE = 0x0200;
    }

    // Helper for VK code lookup
    internal static class VKeyHelper
    {
        private static readonly int[] _vkCodes = new int[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, // 0-9
            0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, // A-F
            0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, // G-O
            0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, // P-Z
            0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, // [ and ]
            0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, // \ and '
            0x3C, 0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, // Enter and
            0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, // Left Ctrl and
            0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, // Left Shift and
            0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F, 0x60, 0x61, 0x62, 0x63, // Left Alt and
            0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, // Space and
            0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, // Caps Lock and
            0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F, 0x80, 0x81, // F1-F24
            0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, // Num Lock and
            0x8C, 0x8D, 0x8E, 0x8F, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, // Scroll Lock and
            0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F, // Home and
            0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, // Arrow keys
            0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF, 0xB0, 0xB1, 0xB2, 0xB3, // Insert and
            0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, // Delete and
            0xBE, 0xBF, 0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, // Page Up and
            0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF, 0xD0, 0xD1, // Page Down and
            0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, // End and
            0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, // Select and
            0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, // Print and
            0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, // Execute and
            0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF // Help and
        };

        public static int GetVKCode(int wParam) => _vkCodes[wParam & 0xFF];
    }
}