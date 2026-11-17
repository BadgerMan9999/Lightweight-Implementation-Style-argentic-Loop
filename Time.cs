// ============================================================================
// Time.cs - [LISL_W]
// High-resolution timing using QueryPerformanceCounter
// ============================================================================

using System;
using System.Diagnostics;

namespace LISL_Core.PAL
{
    /// <summary>
    /// High-resolution time class for delta time calculation.
    /// </summary>
    public sealed class Time : IDisposable
    {
        private static readonly Lazy<Time> _instance = new(() => new Time());
        
        private bool _disposed;

        // Performance counter frequency (ticks per second)
        private static long _frequency;

        public static Time Instance => _instance.Value;

        /// <summary>
        /// Get the current high-resolution time in ticks.
        /// </summary>
        public long GetCurrentTicks() => QueryPerformanceCounter();

        /// <summary>
        /// Get elapsed time since a previous tick in seconds.
        /// </summary>
        public double GetElapsedSeconds(long startTicks)
        {
            var now = QueryPerformanceCounter();
            return (double)(now - startTicks) / _frequency;
        }

        /// <summary>
        /// Get elapsed time since a previous tick in milliseconds.
        /// </summary>
        public double GetElapsedMilliseconds(long startTicks)
        {
            var now = QueryPerformanceCounter();
            return (double)(now - startTicks) / _frequency * 1000;
        }

        /// <summary>
        /// Get elapsed time since a previous tick in microseconds.
        /// </summary>
        public double GetElapsedMicroseconds(long startTicks)
        {
            var now = QueryPerformanceCounter();
            return (double)(now - startTicks) / _frequency * 1000000;
        }

        /// <summary>
        /// Initialize the performance counter frequency.
        /// Call once at application startup.
        /// </summary>
        public static void Initialize()
        {
            if (_frequency == 0)
                QueryPerformanceFrequency(out _frequency);
        }

        private long QueryPerformanceCounter() => 
            PerformanceCounter.CounterValue;

        private static bool QueryPerformanceFrequency(out long freq) => 
            PerformanceCounter.Frequency = freq;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }

        ~Time() => Dispose();
    }

    // Simple performance counter wrapper
    internal static class PerformanceCounter
    {
        private static long _frequency;

        public static bool Initialize(out long freq)
        {
            if (QueryPerformanceFrequency(ref freq))
            {
                _frequency = freq;
                return true;
            }
            
            // Fallback to DateTime ticks if QPC fails
            freq = 10000000; // Approximate fallback
            return false;
        }

        public static long CounterValue => 
            DateTime.UtcNow.Ticks / 10000; // Convert to microseconds

        public static bool QueryPerformanceFrequency(ref long freq) => 
            System.Diagnostics.QueryPerformanceFrequency(out freq);
    }
}