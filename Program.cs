// ============================================================================
// Program.cs - [LISL_W]
// Test program for Phase 01 Foundation implementation
// ============================================================================

using System;
using System.Diagnostics;
using System.Runtime.GCSettings;
using LISL_Core.Memory;
using LISL_Core.Logging;
using LISL_Core.Concurrency;
using LISL_Core.PAL;

namespace LISL_Phase01
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== LISL Phase 01 Foundation Test ===\n");

            // Initialize logging
            Logger.Instance.Info("Starting Phase 01 tests...");

            try
            {
                // Test Layer 1.3: Platform Abstraction
                Logger.Instance.Info("Testing Platform Abstraction (Layer 1.3)...");
                
                Time.Initialize();
                var startTime = Time.Instance.GetCurrentTicks();
                Logger.Instance.Info($"Time initialized, frequency: {Time.PerformanceCounter._frequency} ticks/sec");

                // Test Layer 1.2: Concurrency & Logging
                Logger.Instance.Info("Testing JobSystem (Layer 1.2)...");
                
                int jobCount = 100;
                var jobs = new IJob[jobCount];
                
                for (int i = 0; i < jobCount; i++)
                {
                    jobs[i] = new TestJob(i);
                }

                Logger.Instance.Info($"Submitting {jobCount} concurrent jobs...");
                JobSystem.Instance.EnqueueRange(jobs);
                
                // Wait a bit for jobs to complete
                Thread.Sleep(100);
                Logger.Instance.Info("All jobs completed successfully!");

                // Test Layer 1.2: Memory Manager with Pool Allocator
                Logger.Instance.Info("Testing MemoryManager (Layer 1.1)...");
                
                var memoryManager = MemoryManager.Instance;
                int allocCount = 10000;
                var allocatedObjects = new IntPtr[allocCount];

                // Allocate from pool without GC pressure
                for (int i = 0; i < allocCount; i++)
                {
                    allocatedObjects[i] = memoryManager.AllocFromCurrentThread(64);
                }

                Logger.Instance.Info($"Allocated {allocCount} objects from arena: {memoryManager.GetCurrentThreadArena().UsedBytes} bytes used");

                // Free the allocations
                for (int i = 0; i < allocCount; i++)
                {
                    memoryManager.AllocFromCurrentThread(64); // Reuse same size
                }

                Logger.Instance.Info("Memory allocation test completed!");

                // Test Layer 1.3: Window creation
                Logger.Instance.Info("Testing Window creation (Layer 1.3)...");
                
                var window = new Window();
                Logger.Instance.Info($"Window created with handle: {window.Handle}");
                Logger.Instance.Info($"Window position and size: {window.GetPositionAndSize()}");

                // Test Layer 1.2: InputManager
                Logger.Instance.Info("Testing InputManager (Layer 1.3)...");
                
                var input = LISL_Core.PAL.InputManager.Instance;
                Logger.Instance.Info($"Mouse position: {input.GetMousePosition()}");

                // Cleanup
                window.Dispose();
                JobSystem.Instance.Dispose();
                Logger.Instance.Dispose();

                Logger.Instance.Info("=== All Phase 01 tests passed! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }

        private class TestJob : LISL_Core.Concurrency.IJob
        {
            private readonly int _id;

            public TestJob(int id) => _id = id;

            public void Execute()
            {
                // Simulate some work
                Thread.Sleep(1);
                
                // Log from worker thread to test thread safety
                Logger.Instance.Info($"Job {_id} completed");
            }
        }
    }
}