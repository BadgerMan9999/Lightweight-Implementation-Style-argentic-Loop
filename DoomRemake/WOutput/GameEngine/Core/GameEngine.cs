[EMITTER:E2B_WORKER_01]
[TARGET:DOOMREMAKE_CORE_ENGINE]
[ID: GameEngine.cs]

using System;
using System.Collections.Generic;
using DoomRemake.Core;

namespace DoomRemake.Engine
{
    /// <summary>
    /// Main game engine class for DOOM Remake.
    /// Handles the main game loop, rendering pipeline, and entity management.
    /// </summary>
    public class GameEngine : IDisposable
    {
        private bool _isRunning;
        private float _ deltaTime = 0f;
        private readonly List<Entity> _entities = new();
        
        // Rendering state
        private int _frameCount;
        private double _framesPerSecond = 60.0;

        public event Action<float>? OnUpdate;
        public event Action<int>? OnRender;
        public event Action<Exception>? OnError;

        /// <summary>
        /// Initialize the game engine with default settings.
        /// </summary>
        public void Initialize()
        {
            Console.WriteLine("[INFO] Initializing DOOM Remake Engine...");
            
            // Initialize input hooks
            InputHooks.Initialize();
            
            _isRunning = true;
            Console.WriteLine("[OK] Game engine initialized");
        }

        /// <summary>
        /// Start the main game loop.
        /// </summary>
        public void Run()
        {
            if (!_isRunning) return;
            
            Console.WriteLine("\n=== DOOM REMAKE ===\n");
            Console.WriteLine("Engine: Ready");
            Console.WriteLine("Controls: WASD - Move, SPACE - Jump, SHIFT - Sprint");
            Console.WriteLine("Press ESC to quit...\n");

            while (_isRunning)
            {
                float frameTime = Update();
                Render(frameTime);
                
                _frameCount++;
            }
        }

        /// <summary>
        /// Update game state and return delta time.
        /// </summary>
        private float Update()
        {
            // Calculate delta time (simplified)
            _deltaTime = 1f / _framesPerSecond;
            
            // Process input events
            ProcessInput();
            
            // Update entities
            foreach (var entity in _entities)
            {
                entity.Update(_deltaTime);
            }
            
            OnUpdate?.Invoke(_deltaTime);
            
            return _deltaTime;
        }

        /// <summary>
        /// Render the current frame.
        /// </summary>
        private void Render(float deltaTime)
        {
            // Placeholder for rendering implementation
            Console.WriteLine($"[Frame {_frameCount}] FPS: {_framesPerSecond:F0}");
            
            OnRender?.Invoke(_frameCount);
        }

        /// <summary>
        /// Process input events from InputHooks.
        /// </summary>
        private void ProcessInput()
        {
            // Input processing will be implemented here
            // Integrates with InputHooks class for keyboard/mouse events
        }

        /// <summary>
        /// Register a new entity in the game world.
        /// </summary>
        public Entity AddEntity(string name, EntityType type)
        {
            var entity = new Entity(name, type);
            _entities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Remove an entity from the game world.
        /// </summary>
        public void RemoveEntity(Entity entity)
        {
            if (_entities.Contains(entity))
            {
                _entities.Remove(entity);
            }
        }

        /// <summary>
        /// Cleanup and shutdown the engine.
        /// </summary>
        public void Shutdown()
        {
            _isRunning = false;
            
            // Clean up resources
            InputHooks.Shutdown();
            
            Console.WriteLine("[INFO] Engine shutdown complete");
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Shutdown();
                }
                
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~GameEngine()
        {
            Dispose(false);
        }
        #endregion
    }

    /// <summary>
    /// Entity type enumeration.
    /// </summary>
    public enum EntityType
    {
        Player,
        Enemy,
        Item,
        Environment,
        Projectile
    }

    /// <summary>
    /// Base entity class for game objects.
    /// Implements the Entity-Component System (ECS) pattern foundation.
    /// </summary>
    public class Entity : IDisposable
    {
        private readonly Dictionary<string, object?> _components = new();
        
        public string Name { get; }
        public EntityType Type { get; }
        public float Health { get; set; } = 100f;
        public bool IsActive { get; set; } = true;

        public event Action<Entity, string>? OnComponentChanged;

        public Entity(string name, EntityType type)
        {
            Name = name;
            Type = type;
            
            // Initialize default components based on entity type
            switch (type)
            {
                case EntityType.Player:
                    AddComponent("Position", new Vector2(0f, 0f));
                    AddComponent("Velocity", new Vector2(0f, 0f));
                    Health = 100f;
                    break;
                
                case EntityType.Enemy:
                    AddComponent("HealthBar", 50f);
                    Health = 50f;
                    break;
            }
        }

        /// <summary>
        /// Add a component to the entity.
        /// </summary>
        public void AddComponent<T>(string componentName, T value) where T : notnull
        {
            _components[componentName] = value;
            OnComponentChanged?.Invoke(this, componentName);
        }

        /// <summary>
        /// Get a component by name.
        /// </summary>
        public T? GetComponent<T>(string componentName) where T : notnull
        {
            if (_components.TryGetValue(componentName, out var value))
            {
                return (T)value;
            }
            return default;
        }

        /// <summary>
        /// Update entity state.
        /// </summary>
        public virtual void Update(float deltaTime)
        {
            // Default implementation - subclasses can override
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~Entity()
        {
            Dispose(false);
        }
        #endregion
    }

    /// <summary>
    /// 2D vector structure for position/velocity.
    /// </summary>
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new(0f, 0f);
        
        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float scale) => new(a.X * scale, a.Y * scale);
    }
}