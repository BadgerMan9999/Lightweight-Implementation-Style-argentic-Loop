# LISL Phase 01 Foundation Implementation - [LISL_W]

## Overview
This directory contains the implementation of Phase 01: Foundation for a Vulkan Doom port in C#.

## Status
- ✅ **Layer 1.1**: Memory Architecture (MemoryManager with Arena and Pool allocators)
- ✅ **Layer 1.2**: Concurrency & Logging (Logger, JobSystem)
- ✅ **Layer 1.3**: Platform Abstraction (Window, InputManager, Time)
- ⏳ **Layer 1.4**: Vulkan Bootstrap (Pending)

## Directory Structure

```
LISL_IST/Project/
├── LISL_Core.csproj          # C# project file
├── Program.cs                # Test program
└── README.md                 # This file

LISL_IST/Memory/
└── MemoryManager.cs          # Layer 1.1: Arena and Pool allocators

LISL_IST/Concurrency/
├── Logger.cs                 # Layer 1.2: Thread-safe logging
└── JobSystem.cs              # Layer 1.2: Multi-threaded job system

LISL_IST/PAL/
├── Window.cs                 # Layer 1.3: Win32 windowing system
└── Time.cs                   # Layer 1.3: High-resolution timing
```

## Implementation Details

### MemoryManager (Layer 1.1)
- **Singleton pattern** for global access
- **Arena Allocator**: Rapid allocation with Reset capability
- **Pool Allocator**: O(1) allocation/deallocation using free block linked list
- Uses `NativeMemory.Alloc/Free` for raw memory management
- Thread-local arenas to minimize contention

### Logger (Layer 1.2)
- Thread-safe logging via lock protection
- Outputs to both console and file (`logs/session.log`)
- Supports levels: Info, Warning, Error, Fatal
- Timestamped messages

### JobSystem (Layer 1.2)
- Fixed-size worker thread pool (Logical Cores - 1)
- ConcurrentQueue for thread-safe job submission
- Workers poll the queue continuously with minimal CPU usage
- Proper shutdown handling with timeout

### Window (Layer 1.3)
- Raw Win32 API via P/Invoke (no external libraries)
- Borderless window with Vulkan compatibility flags
- Message loop in background thread
- Key and mouse event logging

### InputManager (Layer 1.3)
- Keyboard state tracking (256 keys)
- Edge detection for key press events
- Mouse position retrieval via Win32 API

### Time (Layer 1.3)
- High-resolution timing using QueryPerformanceCounter
- Elapsed time calculation in seconds/milliseconds/microseconds
- Fallback to DateTime ticks if QPC unavailable

## Building and Testing

```bash
cd LISL_IST/Project
dotnet build
dotnet run
```

Expected output:
- All 100 concurrent jobs complete successfully
- Memory allocations from pool do not trigger GC
- Window opens and logs key presses
- Mouse position is reported correctly

## Next Steps (Layer 1.4)
- VulkanContext class with Instance, Physical Device, Logical Device
- Swapchain initialization
- Command buffer recording for clear screen
- Validation layer callbacks to Logger

## Attribution
Implementation by: [LISL_W]