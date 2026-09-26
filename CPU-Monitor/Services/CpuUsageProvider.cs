using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CPU_Monitor.Services;

/// <summary>
/// Provides CPU usage measurements using platform-specific APIs.
/// Currently supports Windows via <see cref="PerformanceCounter"/>.
/// </summary>
public class CpuUsageProvider : ICpuUsageProvider
{
    private readonly PerformanceCounter? _cpuCounter;

    /// <summary>
    /// Initializes a new instance of the <see cref="CpuUsageProvider"/> class.
    /// </summary>
    /// <remarks>
    /// On Windows, initializes a <see cref="PerformanceCounter"/> for total CPU usage.
    /// On other platforms, CPU usage is not supported.
    /// </remarks>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown if the current operating system is not supported.
    /// </exception>
    public CpuUsageProvider()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            // First call initializes the counter and returns 0, so we discard it
            _cpuCounter.NextValue();
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }

    /// <summary>
    /// Asynchronously measures CPU usage over a given time interval.
    /// </summary>
    /// <param name="interval">The time period over which CPU usage is measured.</param>
    /// <returns>The CPU usage percentage.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown if the current operating system is not supported.
    /// </exception>
    public async Task<double> GetCpuUsageAsync(TimeSpan interval)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Wait for the sampling interval to elapse
            await Task.Delay(interval);

            // Retrieve CPU usage value
            return _cpuCounter!.NextValue() / 10;
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
}