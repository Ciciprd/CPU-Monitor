namespace CPU_Monitor.Services;

/// <summary>
/// Provides a background service that periodically collects system metrics (currently CPU usage)
/// and writes them to log files.
/// </summary>
public class CpuMonitoringService : ICpuMonitoringService
{
    private readonly CpuUsageProvider _cpuProvider = new();
    private readonly string _directoryPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="CpuMonitoringService"/> class.
    /// </summary>
    /// <param name="configuration">
    /// Application configuration used to retrieve the metrics storage directory path
    /// (key: <c>Metrics:DirectoryPath</c>).
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the required configuration value for the metrics directory path is missing.
    /// </exception>
    /// <exception cref="IOException">
    /// Thrown when the directory cannot be created due to an I/O error.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the application does not have permission to create or access the directory.
    /// </exception>
    /// <remarks>
    /// The constructor ensures that the target directory for metric logs exists by creating it if necessary.
    /// This introduces file system access during construction, which may fail if the environment is misconfigured.
    /// </remarks>
    public CpuMonitoringService(IConfiguration configuration)
    {
        _directoryPath = configuration["Metrics:DirectoryPath"]
            ?? throw new InvalidOperationException("Metrics:DirectoryPath is not configured.");

        // Normalize path to avoid issues with relative paths
        _directoryPath = Path.GetFullPath(_directoryPath);

        // Ensure directory exists (safe even if already created)
        Directory.CreateDirectory(_directoryPath);
    }

    /// <summary>
    /// The background task responsible for collecting and logging metrics.
    /// </summary>
    private Task? _runningTask;

    /// <summary>
    /// Cancellation token source used to signal the background task to stop.
    /// </summary>
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Synchronization object to ensure thread-safe access to state.
    /// </summary>
    private readonly Lock _lock = new();

    /// <summary>
    /// Indicates whether the service is currently running.
    /// </summary>
    private bool _isRunning = false;

    /// <summary>
    /// Gets a value indicating whether the metrics collection service is currently running.
    /// </summary>
    public bool IsRunning
    {
        get
        {
            lock (_lock)
            {
                return _isRunning;
            }
        }
    }

    /// <summary>
    /// Starts the metrics collection service.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the service was successfully started;
    /// <c>false</c> if it was already running.
    /// </returns>
    public bool Start()
    {
        lock (_lock) // Ensures Start cannot run concurrently with itself or StopAsync
        {
            if (_isRunning)
                return false; // Already running

            _isRunning = true;
            
            // Create a new cancellation token source for this run
            _cts = new CancellationTokenSource();

            // Start background task on thread pool
            _runningTask = Task.Run(() => RunAsync(_cts.Token));

            return true;
        }
    }

    /// <summary>
    /// Stops the metrics collection service asynchronously.
    /// </summary>
    /// <returns>A task that completes when the background logging task has stopped.</returns>
    public async Task StopAsync()
    {
        Task? taskToWait;

        lock (_lock) // Ensures Stop cannot run concurrently with Start/Stop
        {
            if (!_isRunning)
                return; // Already stopped

            _isRunning = false;

            // Signal cancellation to the running task
            _cts!.Cancel();

            // Capture the task reference to await outside the lock
            taskToWait = _runningTask;
        }

        if (taskToWait != null)
        {
            try
            {
                // Await completion of the background task
                await taskToWait;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }
    }

    /// <summary>
    /// Background loop that periodically collects CPU usage and writes it to a log file.
    /// </summary>
    /// <param name="token">Cancellation token used to stop the loop.</param>
    private async Task RunAsync(CancellationToken token)
    {
        var window = TimeSpan.FromSeconds(5);

        while (!token.IsCancellationRequested)
        {
            // Measure CPU usage over the specified interval
            var cpuUsage = await _cpuProvider.GetCpuUsageAsync(window);
            var cpu = cpuUsage.ToString("F1");

            var now = DateTime.Now;

            // Format: HH:mm:ss;CPU%
            var logLine = $"{now:HH:mm:ss};{cpu}";

            // Daily log file
            var fileName = $"{now:yyyy-MM-dd}.log";
            var directoryPath = _directoryPath;

            var filePath = Path.Combine(directoryPath, fileName);

            // Append log entry
            await File.AppendAllTextAsync(filePath, logLine + Environment.NewLine, token);
        }
    }
}