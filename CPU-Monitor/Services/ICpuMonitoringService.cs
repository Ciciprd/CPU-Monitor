namespace CPU_Monitor.Services;

/// <summary>
/// Defines the contract for a service that periodically collects
/// and logs system CPU usage metrics.
/// </summary>
public interface ICpuMonitoringService
{
    /// <summary>
    /// Gets a value indicating whether the metrics collection service
    /// is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Starts the metrics collection service.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the service was successfully started;
    /// <c>false</c> if it was already running.
    /// </returns>
    bool Start();

    /// <summary>
    /// Stops the metrics collection service asynchronously.
    /// </summary>
    /// <returns>
    /// A task that completes when the background logging task has stopped.
    /// </returns>
    Task StopAsync();
}