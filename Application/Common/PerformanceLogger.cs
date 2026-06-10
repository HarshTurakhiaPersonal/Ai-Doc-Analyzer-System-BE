using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Common;

public sealed class PerformanceLogger(ILogger logger)
{
    public IDisposable Measure(string operationName)
    {
        return new PerformanceScope(logger, operationName);
    }

    private sealed class PerformanceScope(ILogger logger, string operationName) : IDisposable
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public void Dispose()
        {
            _stopwatch.Stop();

            logger.LogInformation("{Operation} completed in {ElapsedMs} ms", operationName, _stopwatch.ElapsedMilliseconds);
        }
    }
}