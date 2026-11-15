[EMITTER:E2B_WORKER_01]
[TARGET:DOOMREMAKE_ASSET_LOADING]
[ID: AssetLoader.cs]

using System;
using System.Collections.Generic;
using System.IO;
using DoomRemake.Core;

namespace DoomRemake.Assets
{
    /// <summary>
    /// Manages loading and management of DOOM game assets from WAD files.
    /// Supports textures, sounds, sprites, and other lump types.
    /// </summary>
    public class AssetLoader : IDisposable
    {
        private readonly Dictionary<string, string> _wadPaths = new();
        private bool _isInitialized;

        public event Action<string, int>? OnAssetLoaded;
        public event Action<string, Exception>? OnLoadError;

        /// <summary>
        /// Initialize the asset loader.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                // Set up default assets directory
                string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "assets");
                
                _wadPaths["default"] = Path.Combine(assetsPath, "wads", "doom2.wad");
                
                Console.WriteLine("[INFO] AssetLoader initialized");
                Console.WriteLine($"[INFO] Assets path: {assetsPath}");
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to initialize AssetLoader: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load a WAD file.
        /// </summary>
        public void LoadWad(string wadPath, string name = "default")
        {
            if (!_isInitialized) Initialize();

            try
            {
                if (!File.Exists(wadPath))
                {
                    throw new FileNotFoundException($"WAD file not found: {wadPath}");
                }

                _wadPaths[name] = wadPath;
                
                // Load all lumps from the WAD
                var lumps = WadFileHandler.ListLumps(wadPath);
                
                Console.WriteLine($"[INFO] Loaded WAD '{name}' with {lumps.Count} lumps");
            }
            catch (Exception ex)
            {
                OnLoadError?.Invoke(name, ex);
                throw;
            }
        }

        /// <summary>
        /// Load a texture from a loaded WAD.
        /// </summary>
        public byte[]? LoadTexture(string wadName = "default", string textureName = null)
        {
            if (!_isInitialized) Initialize();

            try
            {
                string wadPath = _wadPaths[wadName];
                
                // Filter for texture lumps (TEX_* pattern)
                var textures = GetTextures(wadName);
                
                foreach (var tex in textures)
                {
                    if (string.IsNullOrEmpty(textureName) || 
                        tex.Contains(textureName, StringComparison.OrdinalIgnoreCase))
                    {
                        var data = WadFileHandler.LoadLump(wadPath, tex);
                        
                        if (data != null)
                        {
                            OnAssetLoaded?.Invoke(tex, data.Length);
                            return data;
                        }
                    }
                }

                Console.WriteLine($"[WARN] Texture '{textureName}' not found in WAD");
                return null;
            }
            catch (Exception ex)
            {
                OnLoadError?.Invoke(wadName, ex);
                return null;
            }
        }

        /// <summary>
        /// Get list of all textures in a WAD.
        /// </summary>
        public string[] GetTextures(string wadName = "default")
        {
            if (!_isInitialized) Initialize();

            try
            {
                string wadPath = _wadPaths[wadName];
                return WadFileHandler.ListLumps(wadPath).Where(l => l.Contains("TEX")).ToArray();
            }
            catch (Exception ex)
            {
                OnLoadError?.Invoke(wadName, ex);
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Load a sound from a loaded WAD.
        /// </summary>
        public byte[]? LoadSound(string wadName = "default", string soundName = null)
        {
            if (!_isInitialized) Initialize();

            try
            {
                string wadPath = _wadPaths[wadName];
                
                // Filter for sound lumps (SND_* pattern)
                var sounds = GetSounds(wadName);
                
                foreach (var snd in sounds)
                {
                    if (string.IsNullOrEmpty(soundName) || 
                        snd.Contains(soundName, StringComparison.OrdinalIgnoreCase))
                    {
                        var data = WadFileHandler.LoadLump(wadPath, snd);
                        
                        if (data != null)
                        {
                            OnAssetLoaded?.Invoke(snd, data.Length);
                            return data;
                        }
                    }
                }

                Console.WriteLine($"[WARN] Sound '{soundName}' not found in WAD");
                return null;
            }
            catch (Exception ex)
            {
                OnLoadError?.Invoke(wadName, ex);
                return null;
            }
        }

        /// <summary>
        /// Get list of all sounds in a WAD.
        /// </summary>
        public string[] GetSounds(string wadName = "default")
        {
            if (!_isInitialized) Initialize();

            try
            {
                string wadPath = _wadPaths[wadName];
                return WadFileHandler.ListLumps(wadPath).Where(l => l.Contains("SND")).ToArray();
            }
            catch (Exception ex)
            {
                OnLoadError?.Invoke(wadName, ex);
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Load a sprite from a WAD file.
        /// </summary>
        public byte[]? LoadSprite(string wadName = "default", string spriteName = null)
        {
            if (!_isInitialized) Initialize();

            try
            {
                string wadPath = _wadPaths[wadName];
                
                // Filter for sprite lumps (SPR_* pattern)
                var sprites = WadFileHandler.ListLumps(wadPath).Where(l => l.Contains("SPR"));
                
                foreach (var spr in sprites)
                {
                    if (string.IsNullOrEmpty(spriteName) || 
                        spr.Contains(spriteName, StringComparison.OrdinalIgnoreCase))
                    {
                        var data = WadFileHandler.LoadLump(wadPath, spr);
                        
                        if (data != null)
                        {
                            OnAssetLoaded?.Invoke(spr, data.Length);
                            return data;
                        }
                    }
                }

                Console.WriteLine($"[WARN] Sprite '{spriteName}' not found in WAD");
                return null;
            }
            catch (Exception ex)
            {
                OnLoadError?.Invoke(wadName, ex);
                return null;
            }
        }

        /// <summary>
        /// Get all available assets from a WAD.
        /// </summary>
        public Dictionary<string, string[]> GetAllAssets(string wadName = "default")
        {
            if (!_isInitialized) Initialize();

            try
            {
                string wadPath = _wadPaths[wadName];
                
                var assets = new Dictionary<string, string[]>
                {
                    ["textures"] = GetTextures(wadName),
                    ["sounds"] = GetSounds(wadName),
                    ["sprites"] = WadFileHandler.ListLumps(wadPath).Where(l => l.Contains("SPR")).ToArray()
                };

                return assets;
            }
            catch (Exception ex)
            {
                OnLoadError?.Invoke(wadName, ex);
                return new Dictionary<string, string[]>();
            }
        }

        /// <summary>
        /// Get available WAD files.
        /// </summary>
        public List<string> GetAvailableWads()
        {
            var wads = new List<string>();
            
            foreach (var kvp in _wadPaths)
            {
                if (File.Exists(kvp.Value))
                {
                    wads.Add(Path.GetFileName(kvp.Value));
                }
            }

            return wads;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Clean up any loaded assets here
                }
                
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~AssetLoader()
        {
            Dispose(false);
        }
        #endregion
    }

    /// <summary>
    /// Asset metadata structure.
    /// </summary>
    public class AssetMetadata
    {
        public string Name { get; set; } = "";
        public int Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Type { get; set; } = "unknown";

        public static AssetMetadata Create(string name, int size) => 
            new() { Name = name, Size = size, LastModified = DateTime.UtcNow };
    }
}