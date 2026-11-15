using System;
using System.Runtime.InteropServices;
using DoomRemake.Core;

namespace DoomRemake
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== DOOM REMAKE ===");
            Console.WriteLine(".NET 9 Build - Multiplayer Ready");
            
            // Initialize Windows Input Hooks (User32.dll)
            try
            {
                InputHooks.Initialize();
                Console.WriteLine("[OK] Windows Input hooks initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Input initialization failed: {ex.Message}");
            }

            // Initialize WAD File I/O Handlers
            try
            {
                WadFileHandler.Initialize();
                Console.WriteLine("[OK] WAD file handlers initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] WAD handler initialization failed: {ex.Message}");
            }

            // Display menu or start application logic here
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}