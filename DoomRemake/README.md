# DOOM REMAKE

A modern C# .NET 9 remake of the classic DOOM game with native multiplayer hosting capabilities designed to run on free cloud server tiers (Fly.io, Render, Oracle Cloud Free Tier).

## Project Structure

```
DoomRemake/
├── src/                    # Source code
│   ├── DoomRemake.csproj  # .NET 9.0 project file
│   ├── Program.cs         # Application entry point
│   └── Core/              # Core subsystems
│       ├── InputHooks.cs  # Windows API input capture
│       ├── WadFileHandler.cs  # WAD lump reading utilities
│       └── WadLoader.cs   # Asset loading utilities
├── assets/                # Game assets
│   ├── wads/             # DOOM WAD files
│   ├── textures/         # Texture packs
│   └── sounds/           # Sound effects and music
├── docs/                  # Documentation
│   └── LISL_Specs/       # LISL protocol specifications
├── logs/                  # Runtime logs
├── DMI/                   # Dev Manager Instructions (read-only)
├── WOI/                   # Worker Order Instructions (input from DEV_MANAGER)
└── WOutput/               # Worker Output (generated code/artifacts)
```

## Build Requirements

- **.NET 9.0 SDK** or higher
- **Windows OS** (for User32.dll input hooks)
- Required NuGet packages are specified in `DoomRemake.csproj`

## Building the Project

```bash
cd src/DoomRemake
dotnet restore
dotnet build
```

## Running the Application

```bash
cd src/DoomRemake
dotnet run
```

## LISL Sharding Protocol

This project uses a **Logic Inference Script Language (LISL)** sharding protocol for task automation:

### Directory Roles

| Directory | Purpose | Access |
|-----------|---------|--------|
| `DMI/` | Dev Manager Instructions - High-level task definitions | DEV_MANAGER writes, workers read |
| `WOI/` | Worker Order Instructions - Sequential specification files | DEV_MANAGER writes, workers execute |
| `WOutput/` | Worker Output - Generated code and build artifacts | Workers write, DEV_MANAGER reads |

### Workflow

1. **DEV_MANAGER** writes task specifications to `WOI/DoomRemake_Spec_XX.xml`
2. **WORKER** reads specification from `WOI/`, executes tasks, outputs results to `WOutput/`
3. **WORKER** creates `Finished.md` in `WOutput/` when complete
4. **DEV_MANAGER** monitors for `Finished.md`, then verifies output

### File Signatures

All files must include proper `[EMITTER:...]` tags at the beginning to identify their source and purpose.

## Core Subsystems

### InputHooks (User32.dll Integration)

Captures Windows keyboard and mouse events through low-level hooks:
- Keyboard: Key press/release detection with virtual key codes
- Mouse: Click events, movement tracking, wheel scrolling
- Thread-safe initialization and shutdown management

### WadFileHandler

Provides standard file I/O for DOOM WAD files:
- Supports PWAD/DOOM format magic numbers
- Parses lump directory entries (name, size, file position)
- Loads individual lumps by name with error handling
- Lists all available assets in a WAD file

### WadLoader

Utility class for loading specific asset types:
- `LoadTexture()` - Load texture lumps from WAD files
- `GetTextures()` - List all textures in a WAD
- `LoadSound()` - Load sound effect lumps
- `GetSounds()` - List all sounds in a WAD

## Adding WAD Files

Place your DOOM WAD files in the `assets/wads/` directory. The application will automatically discover and load available assets.

Example WAD file structure:
```
assets/wads/
├── doom2.wad        # Full DOOM II game data
├── plr1.zip         # Player 1 textures
└── sounds/doom.snd  # Custom sound pack
```

## Logging

Runtime logs are written to the `logs/` directory. Each subsystem generates its own log files with timestamps and error information.

## Development Status

- [x] Project structure initialized
- [x] .NET 9.0 configuration complete
- [x] Core subsystems implemented
- [x] LISL sharding protocol established
- [ ] Multiplayer networking implementation
- [ ] Game engine integration
- [ ] UI/HUD rendering

## License

BadgerSoft Productions - All Rights Reserved

---

**EMITTER: DEV_MANAGER_12B_QAT**  
**PROJECT_INIT: DOOMREMAKE_V1.0**