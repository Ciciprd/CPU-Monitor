namespace CPU_Monitor.Services;

/// <summary>
/// Provides functionality for measuring the current CPU usage.
/// </summary>
public interface ICpuUsageProvider
{
    /// <summary>
    /// Measures the CPU usage over the specified time interval.
    /// </summary>
    /// <param name="interval">
    /// The time interval over which the CPU usage is measured.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous CPU usage measurement operation.
    /// The task result contains the CPU usage as a percentage in the range
    /// from 0 to 100.
    /// </returns>
    Task<double> GetCpuUsageAsync(TimeSpan interval);
}