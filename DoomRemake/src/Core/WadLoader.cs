using System;
using System.IO;
using DoomRemake.Core;

namespace DoomRemake.Core
{
    /// <summary>
    /// Utility class for loading WAD files and extracting assets.
    /// Provides methods to load textures, sprites, sounds from DOOM WAD files.
    /// </summary>
    public static class WadLoader
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// Load a specific lump type from a WAD file.
        /// </summary>
        public static byte[]? LoadTexture(string wadPath, string textureName)
        {
            if (!_isInitialized) Initialize();
            
            return WadFileHandler.LoadLump(wadPath, textureName);
        }

        /// <summary>
        /// List all available textures in a WAD file.
        /// </summary>
        public static string[] GetTextures(string wadPath)
        {
            if (!_isInitialized) Initialize();
            
            var lumps = WadFileHandler.ListLumps(wadPath);
            return lumps.Where(l => l.Contains("TEX")).ToArray();
        }

        /// <summary>
        /// Load a sound lump from a WAD file.
        /// </summary>
        public static byte[]? LoadSound(string wadPath, string soundName)
        {
            if (!_isInitialized) Initialize();
            
            return WadFileHandler.LoadLump(wadPath, soundName);
        }

        /// <summary>
        /// List all available sounds in a WAD file.
        /// </summary>
        public static string[] GetSounds(string wadPath)
        {
            if (!_isInitialized) Initialize();
            
            var lumps = WadFileHandler.ListLumps(wadPath);
            return lumps.Where(l => l.Contains("SND")).ToArray();
        }
    }

    /// <summary>
    /// Exception thrown when WAD file operations fail.
    /// </summary>
    public class WadException : Exception
    {
        public WadException(string message) : base(message) { }
        public WadException(string message, Exception inner) : base(message, inner) { }
    }
}