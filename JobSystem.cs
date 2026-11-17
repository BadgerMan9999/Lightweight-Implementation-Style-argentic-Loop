// ============================================================================
// JobSystem.cs - [LISL_W]
// Multi-threaded job system with fixed-size worker thread pool
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LISL_Core.Concurrency
{
    /// <summary>
    /// Interface for job tasks.
    /// </summary>
    public interface IJob
    {
        void Execute();
    }

    /// <summary>
    /// Job system with fixed-size worker thread pool and thread-safe task queue.
    /// </summary>
    public sealed class JobSystem : IDisposable
    {
        private static readonly Lazy<JobSystem> _instance = new(() => new JobSystem());
        
        // Worker threads
        private readonly Thread[] _workers;
        private readonly ConcurrentQueue<IJob> _jobQueue;
        
        // Synchronization primitives
        private readonly ManualResetEventSlim _stopRequested;
        private readonly SemaphoreSlim _shutdownLock;
        private readonly object _queueLock = new object();
        
        private bool _running;
        private volatile bool _disposed;

        public static JobSystem Instance => _instance.Value;

        // Initialize with (Logical Cores - 1) workers
        private int _workerCount;

        public JobSystem()
        {
            var logicalCores = Environment.ProcessorCount;
            _workerCount = Math.Max(1, logicalCores - 1);
            
            _jobQueue = new ConcurrentQueue<IJob>();
            _stopRequested = new ManualResetEventSlim();
            _shutdownLock = new SemaphoreSlim(1, 1);

            // Start worker threads
            _workers = new Thread[_workerCount];
            for (int i = 0; i < _workerCount; i++)
            {
                _workers[i] = new Thread(WorkerLoop)
                {
                    Name = $"LISL_Worker_{i}",
                    IsBackground = true
                };
                _workers[i].Start();
            }

            Logger.Instance.Info($"JobSystem initialized with {_workerCount} worker threads");
        }

        /// <summary>
        /// Submit a job to the queue. Thread-safe via ConcurrentQueue.
        /// </summary>
        public void Enqueue(IJob job)
        {
            if (_disposed || !_running)
                throw new InvalidOperationException("JobSystem not running or disposed");

            _jobQueue.Enqueue(job);
            
            // Signal workers to check queue (they poll continuously)
            WakeUpWorkers();
        }

        /// <summary>
        /// Submit multiple jobs.
        /// </summary>
        public void EnqueueRange(params IJob[] jobs)
        {
            foreach (var job in jobs)
                Enqueue(job);
        }

        /// <summary>
        /// Start the job system.
        /// </summary>
        public void Start() => _running = true;

        /// <summary>
        /// Stop accepting new jobs and wait for completion.
        /// </summary>
        public async Task ShutdownAsync(int timeoutMs = 5000)
        {
            if (_disposed) return;

            _running = false;
            
            // Wait for all workers to finish
            var tasks = _workers.Select(t => t.Join(timeoutMs));
            await Task.WhenAll(tasks);

            Logger.Instance.Info("JobSystem shutdown complete");
        }

        /// <summary>
        /// Worker thread loop. Continuously polls the job queue.
        /// </summary>
        private void WorkerLoop()
        {
            while (!_disposed)
            {
                IJob? job = null;
                
                // Lock-free dequeue attempt
                if (_jobQueue.TryDequeue(out job))
                {
                    try
                    {
                        job.Execute();
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error($"Job execution failed: {ex.Message}");
                        Logger.Instance.Error(ex.ToString());
                    }
                }

                // Small yield to prevent CPU spinning when queue is empty
                Thread.Yield();
            }
        }

        /// <summary>
        /// Wake up all workers (they'll check the queue).
        /// </summary>
        private void WakeUpWorkers()
        {
            _stopRequested.Reset(); // Reset to signaled state
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            
            // Signal shutdown
            _running = false;
            _stopRequested.Set();

            try
            {
                ShutdownAsync(3000).Wait();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Shutdown error: {ex.Message}");
            }
        }

        ~JobSystem() => Dispose();
    }
}