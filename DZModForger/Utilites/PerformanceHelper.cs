using System;
using System.Diagnostics;

namespace DZModForger.Utilities
{
    /// <summary>
    /// Performance measurement and profiling helper
    /// </summary>
    public class PerformanceHelper : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _operationName;
        private readonly bool _logToDebug;

        public PerformanceHelper(string operationName, bool logToDebug = true)
        {
            _operationName = operationName;
            _logToDebug = logToDebug;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets elapsed time in milliseconds
        /// </summary>
        public double ElapsedMilliseconds => _stopwatch.Elapsed.TotalMilliseconds;

        /// <summary>
        /// Gets elapsed time
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        /// <summary>
        /// Stops and logs performance data
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();

            string message = $"[PERF] {_operationName}: {_stopwatch.ElapsedMilliseconds}ms";

            if (_logToDebug)
                Debug.WriteLine(message);

            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// Returns performance message without logging
        /// </summary>
        public override string ToString()
        {
            return $"{_operationName}: {_stopwatch.ElapsedMilliseconds}ms";
        }
    }

    /// <summary>
    /// Quick performance benchmark
    /// </summary>
    public static class BenchmarkHelper
    {
        /// <summary>
        /// Benchmarks an action N times and returns average time
        /// </summary>
        public static double Benchmark(Action action, int iterations = 1000)
        {
            if (action == null || iterations <= 0)
                return 0;

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
                action();

            stopwatch.Stop();

            double averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
            Debug.WriteLine($"[BENCHMARK] Average: {averageMs:F4}ms over {iterations} iterations");

            return averageMs;
        }

        /// <summary>
        /// Benchmarks a function N times and returns average time
        /// </summary>
        public static double Benchmark<T>(Func<T> func, int iterations = 1000)
        {
            if (func == null || iterations <= 0)
                return 0;

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
                func();

            stopwatch.Stop();

            double averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
            Debug.WriteLine($"[BENCHMARK] Average: {averageMs:F4}ms over {iterations} iterations");

            return averageMs;
        }
    }
}
