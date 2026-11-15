using System;
using System.IO;
using System.Collections.Generic;

namespace DoomRemake.Core
{
    /// <summary>
    /// Handler for WAD (WAd file) lump reading and management.
    /// Used for loading DOOM game assets, textures, sounds, etc.
    /// </summary>
    public static class WadFileHandler
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize standard Windows File I/O handlers for WAD lump reading.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                // Set up file system paths for WAD files
                string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "assets");
                
                // Create assets directory if it doesn't exist
                if (!Directory.Exists(assetsPath))
                {
                    Directory.CreateDirectory(assetsPath);
                }

                _isInitialized = true;
                Console.WriteLine("[INFO] WAD file handlers initialized. Assets path: " + assetsPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] WAD handler initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load a lump from a WAD file.
        /// </summary>
        public static byte[]? LoadLump(string wadPath, string lumpName)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            try
            {
                if (!File.Exists(wadPath))
                {
                    throw new FileNotFoundException($"WAD file not found: {wadPath}");
                }

                using (FileStream fs = File.OpenRead(wadPath))
                {
                    WADHeader header = ReadHeader(fs);
                    
                    for (int i = 0; i < header.numLumps; i++)
                    {
                        LUMPINFO lumpInfo = header.lumpInfo[i];
                        
                        if (string.Equals(lumpInfo.name, lumpName, StringComparison.OrdinalIgnoreCase))
                        {
                            fs.Seek(header.fileOffset + lumpInfo.filePos, SeekOrigin.Begin);
                            
                            int dataSize = lumpInfo.size;
                            byte[] data = new byte[dataSize];
                            fs.Read(data, 0, dataSize);
                            
                            return data;
                        }
                    }

                    throw new Exception($"Lump '{lumpName}' not found in WAD file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load lump: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// List all lumps available in a WAD file.
        /// </summary>
        public static List<string> ListLumps(string wadPath)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            try
            {
                if (!File.Exists(wadPath))
                {
                    throw new FileNotFoundException($"WAD file not found: {wadPath}");
                }

                using (FileStream fs = File.OpenRead(wadPath))
                {
                    WADHeader header = ReadHeader(fs);
                    
                    List<string> lumps = new List<string>();
                    for (int i = 0; i < header.numLumps && i < header.lumpInfo.Length; i++)
                    {
                        LUMPINFO lumpInfo = header.lumpInfo[i];
                        lumps.Add(lumpInfo.name);
                    }

                    return lumps;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to list lumps: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Read WAD file header.
        /// </summary>
        private static WADHeader ReadHeader(FileStream fs)
        {
            // Read magic number "PWAD" or "DOOM"
            byte[] magic = new byte[4];
            fs.Read(magic, 0, 4);

            if (System.Text.Encoding.ASCII.GetString(magic) != "PWAD" && 
                System.Text.Encoding.ASCII.GetString(magic) != "DOOM")
            {
                throw new Exception("Invalid WAD file: Invalid magic number");
            }

            // Read header info
            byte[] numLumpsBytes = new byte[4];
            fs.Read(numLumpsBytes, 0, 4);
            int numLumps = System.BitConverter.ToInt32(numLumpsBytes, 0);

            byte[] dirOffsetBytes = new byte[4];
            fs.Read(dirOffsetBytes, 0, 4);
            int dirOffset = System.BitConverter.ToInt32(dirOffsetBytes, 0);

            // Read directory entries (16 bytes each)
            byte[] lumpInfoData = new byte[numLumps * 16];
            fs.Seek(dirOffset, SeekOrigin.Begin);
            fs.Read(lumpInfoData, 0, lumpInfoData.Length);

            return new WADHeader
            {
                magic = System.Text.Encoding.ASCII.GetString(magic),
                numLumps = numLumps,
                fileOffset = 124, // Standard DOOM WAD offset
                lumpInfo = ParseLumpInfo(lumpInfoData)
            };
        }

        /// <summary>
        /// Parse lump info array from binary data.
        /// </summary>
        private static LUMPINFO[] ParseLumpInfo(byte[] data)
        {
            List<LUMPINFO> result = new List<LUMPINFO>();
            
            for (int i = 0; i < data.Length / 16; i++)
            {
                int offset = i * 16;
                
                string name = System.Text.Encoding.ASCII.GetString(data, offset, 8).Trim('\0');
                int size = System.BitConverter.ToInt32(data, offset + 8);
                int filePos = System.BitConverter.ToInt32(data, offset + 12);

                result.Add(new LUMPINFO { name = name, size = size, filePos = filePos });
            }

            return result.ToArray();
        }
    }

    /// <summary>
    /// WAD file header structure.
    /// </summary>
    public struct WADHeader
    {
        public string magic;
        public int numLumps;
        public int fileOffset;
        public LUMPINFO[] lumpInfo;
    }

    /// <summary>
    /// Individual lump information from WAD.
    /// </summary>
    public struct LUMPINFO
    {
        public string name;
        public int size;
        public int filePos;
    }
}