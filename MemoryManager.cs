// ============================================================================
// MemoryManager.cs - [LISL_W]
// Memory Manager Singleton for deterministic, GC-free allocations
// ============================================================================

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace LISL_Core.Memory
{
    /// <summary>
    /// Thread-local storage for per-thread arena pointers.
    /// </summary>
    internal static class ThreadLocalArena
    {
        private static readonly ThreadLocal<IntPtr> _arenas = new();
        
        public static IntPtr Get() => _arenas.Value;
        public static void Set(IntPtr arena) => _arenas.Value = arena;
        public static bool HasValue => _arenas.IsValueCreated;
    }

    /// <summary>
    /// Free block node for the Pool Allocator's free list.
    /// </summary>
    internal struct FreeBlock
    {
        public IntPtr NextFree;
        public int Size;
        
        public static FreeBlock Default => new() { NextFree = IntPtr.Zero, Size = 0 };
    }

    /// <summary>
    /// Arena Allocator: Rapid allocation for short-lived objects sharing a lifetime.
    /// </summary>
    internal class ArenaAllocator
    {
        private readonly IntPtr _buffer;
        private int _offset;
        private readonly int _capacity;

        public ArenaAllocator(int capacityInBytes = 1024 * 1024) // Default 1MB
        {
            _buffer = NativeMemory.Alloc(capacityInBytes);
            _offset = 0;
            _capacity = capacityInBytes;
        }

        /// <summary>
        /// Allocate memory in the arena. Returns IntPtr to raw buffer.
        /// </summary>
        public unsafe IntPtr Alloc(int size)
        {
            if (_offset + size > _capacity)
                throw new InvalidOperationException("Arena exhausted");

            var ptr = (byte*)_buffer + _offset;
            _offset += size;
            return new IntPtr(ptr);
        }

        /// <summary>
        /// Reset the arena to its initial state. All previous allocations are invalidated.
        /// </summary>
        public void Reset()
        {
            if (_buffer != IntPtr.Zero)
                NativeMemory.Free(_buffer);
            
            _buffer = NativeMemory.Alloc(_capacity);
            _offset = 0;
        }

        /// <summary>
        /// Get the total allocated bytes.
        /// </summary>
        public int UsedBytes => _offset;

        /// <summary>
        /// Dispose and free all arena memory.
        /// </summary>
        public void Dispose()
        {
            if (_buffer != IntPtr.Zero)
                NativeMemory.Free(_buffer);
            _buffer = IntPtr.Zero;
            _offset = 0;
        }

        ~ArenaAllocator() => Dispose();
    }

    /// <summary>
    /// Pool Allocator: O(1) allocation for fixed-size objects using free block linked list.
    /// </summary>
    internal class PoolAllocator<T> where T : struct
    {
        private readonly IntPtr _poolBuffer;
        private readonly int _objectSize;
        private readonly int _capacity;
        
        // Free list head (points to first available slot)
        private int _freeListHead = -1;
        private int _nextFreeIndex = 0;
        
        // Track allocated slots for deallocation
        private bool[] _isAllocated;

        public PoolAllocator(int capacity, int objectSizeInBytes)
        {
            _capacity = capacity;
            _objectSize = objectSizeInBytes;
            
            var totalPoolSize = capacity * objectSizeInBytes + sizeof(int); // Extra space for free list
            
            _poolBuffer = NativeMemory.Alloc((int)totalPoolSize);
            
            // Initialize all slots as free (linked list)
            unsafe
            {
                byte* ptr = (byte*)_poolBuffer;
                
                // First slot points to itself (sentinel)
                int* nextFreePtr = (int*)(ptr + sizeof(int));
                *nextFreePtr = 0;
                
                for (int i = 1; i < capacity; i++)
                {
                    int* nextFreePtrI = (int*)(ptr + i * objectSizeInBytes);
                    *nextFreePtrI = i - 1; // Point to previous slot
                }
            }
            
            _freeListHead = 0;
            _nextFreeIndex = capacity;
            _isAllocated = new bool[capacity];
        }

        /// <summary>
        /// Allocate a new object. O(1) via free list traversal.
        /// Returns IntPtr to the allocated slot's memory.
        /// </summary>
        public unsafe IntPtr Alloc()
        {
            if (_freeListHead == -1)
                throw new InvalidOperationException("Pool exhausted");

            int index = _freeListHead;
            _freeListHead = _nextFreeIndex++;
            
            // Mark as allocated
            _isAllocated[index] = true;
            
            return new IntPtr(_poolBuffer + index * _objectSize);
        }

        /// <summary>
        /// Deallocate an object. O(1) via free list insertion.
        /// </summary>
        public unsafe void Free(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero || !_isAllocated[0]) // Index 0 is sentinel
                return;

            int index = ((int)(ptr - _poolBuffer)) / _objectSize;
            
            if (!_isAllocated[index])
                throw new InvalidOperationException("Invalid free: slot not allocated");
            
            // Mark as free
            _isAllocated[index] = false;
            
            // Insert at head of free list
            unsafe
            {
                int* nextFreePtr = (int*)(ptr + _objectSize);
                *nextFreePtr = index;
                
                // Update free list head to point to this slot
                int currentHead = 0;
                while (_freeListHead != -1 && _isAllocated[_freeListHead])
                    _freeListHead = _nextFreeIndex++;
                
                if (_freeListHead == -1)
                    _freeListHead = index;
            }
        }

        /// <summary>
        /// Reset the pool to its initial state. All slots become free again.
        /// </summary>
        public void Reset()
        {
            // Rebuild free list: each slot points to previous
            unsafe
            {
                byte* ptr = (byte*)_poolBuffer;
                
                int* nextFreePtr = (int*)(ptr + sizeof(int));
                *nextFreePtr = 0;
                
                for (int i = 1; i < _capacity; i++)
                {
                    int* nextFreePtrI = (int*)(ptr + i * _objectSize);
                    *nextFreePtrI = i - 1;
                }
            }
            
            _freeListHead = 0;
            _nextFreeIndex = _capacity;
            
            // Reset allocation tracking
            for (int i = 0; i < _capacity; i++)
                _isAllocated[i] = false;
        }

        /// <summary>
        /// Get the total allocated bytes.
        /// </summary>
        public int UsedBytes => _nextFreeIndex * _objectSize;

        /// <summary>
        /// Dispose and free all pool memory.
        /// </summary>
        public void Dispose()
        {
            if (_poolBuffer != IntPtr.Zero)
                NativeMemory.Free(_poolBuffer);
            _poolBuffer = IntPtr.Zero;
        }

        ~PoolAllocator() => Dispose();
    }

    /// <summary>
    /// Memory Manager Singleton providing unified memory allocation interface.
    /// </summary>
    public sealed class MemoryManager
    {
        private static readonly Lazy<MemoryManager> _instance = 
            new(() => new MemoryManager());
        
        public static MemoryManager Instance => _instance.Value;

        // Global pool for fixed-size objects (e.g., Projectiles, Entities)
        private readonly PoolAllocator<EntityData> _entityPool;
        private readonly PoolAllocator<ProjectileData> _projectilePool;
        
        // Thread-local arenas for per-thread allocations
        private readonly ArenaAllocator[] _threadArenas;
        private readonly int _arenaCount;

        public MemoryManager()
        {
            // Initialize pools with reasonable sizes
            const int POOL_CAPACITY = 1024;
            const int ENTITY_SIZE = sizeof(float) * 8 + sizeof(int); // Example: position, velocity, etc.
            const int PROJECTILE_SIZE = sizeof(float) * 6 + sizeof(double); // Position, velocity, lifetime

            _entityPool = new PoolAllocator<EntityData>(POOL_CAPACITY, ENTITY_SIZE);
            _projectilePool = new PoolAllocator<ProjectileData>(POOL_CAPACITY, PROJECTILE_SIZE);

            // Initialize thread-local arenas (one per logical core)
            var processorCount = Environment.ProcessorCount;
            _arenaCount = Math.Max(1, processorCount - 1);
            _threadArenas = new ArenaAllocator[_arenaCount];
            
            for (int i = 0; i < _arenaCount; i++)
                _threadArenas[i] = new ArenaAllocator();
        }

        /// <summary>
        /// Allocate from the default pool (Entity or Projectile based on type).
        /// </summary>
        public unsafe T* Memory_Allocate<T>() where T : unmanaged, IDefaultPoolType
        {
            return (T*)GetPoolForType<T>().Alloc();
        }

        /// <summary>
        /// Allocate from the current thread's arena.
        /// </summary>
        public unsafe IntPtr AllocFromCurrentThread(int size)
        {
            int threadIndex = Thread.CurrentThread.ManagedThreadId % _arenaCount;
            return _threadArenas[threadIndex].Alloc(size);
        }

        /// <summary>
        /// Get the current thread's arena.
        /// </summary>
        public ArenaAllocator GetCurrentThreadArena()
        {
            int threadIndex = Thread.CurrentThread.ManagedThreadId % _arenaCount;
            return _threadArenas[threadIndex];
        }

        /// <summary>
        /// Free memory back to the pool.
        /// </summary>
        public unsafe void Memory_Free<T>(T* ptr) where T : unmanaged, IDefaultPoolType
        {
            GetPoolForType<T>().Free(new IntPtr(ptr));
        }

        /// <summary>
        /// Reset all pools and arenas to initial state.
        /// </summary>
        public void ResetAll()
        {
            _entityPool.Reset();
            _projectilePool.Reset();
            
            for (int i = 0; i < _arenaCount; i++)
                _threadArenas[i].Reset();
        }

        private PoolAllocator<T> GetPoolForType<T>() where T : unmanaged, IDefaultPoolType =>
            typeof(T) == typeof(EntityData) ? _entityPool : 
            typeof(T) == typeof(ProjectileData) ? _projectilePool :
            throw new InvalidOperationException($"Unknown pool type: {typeof(T).Name}");

        ~MemoryManager()
        {
            ResetAll();
        }
    }

    // Marker interface for types that use specific pools
    internal interface IDefaultPoolType { }

    // Example fixed-size data structures
    [StructLayout(LayoutKind.Sequential, Size = sizeof(float) * 8 + sizeof(int))]
    internal struct EntityData : IDefaultPoolType
    {
        public float PositionX;
        public float PositionY;
        public float VelocityX;
        public float VelocityY;
        public int EntityType;
        // ... more fields
    }

    [StructLayout(LayoutKind.Sequential, Size = sizeof(float) * 6 + sizeof(double))]
    internal struct ProjectileData : IDefaultPoolType
    {
        public float PositionX;
        public float PositionY;
        public float VelocityX;
        public float VelocityY;
        public double Lifetime;
        // ... more fields
    }

    /// <summary>
    /// Native memory allocation wrapper.
    /// </summary>
    internal static class NativeMemory
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFree(
            IntPtr lpAddress,
            uint dwSize,
            uint dwFreeType);

        public static IntPtr Alloc(int size) => 
            (IntPtr)VirtualAlloc(IntPtr.Zero, (uint)size, 0x1000, 0x40); // MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE

        public static void Free(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero && VirtualFree(ptr, 0, 0x8000)) // MEM_RELEASE
                ;
        }
    }
}