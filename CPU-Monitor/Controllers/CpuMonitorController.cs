using CPU_Monitor.Models;
using CPU_Monitor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CPU_Monitor.Controllers;

/// <summary>
/// Controller responsible for displaying CPU logs and controlling the CPU logging service.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CpuMonitorController"/> class.
/// </remarks>
/// <param name="service">Service responsible for CPU metrics collection.</param>
/// <param name="logger">Logger instance used for error reporting.</param>
/// <param name="configuration">Application configuration.</param>
/// <exception cref="InvalidOperationException">
/// Thrown when the required configuration key <c>Metrics:DirectoryPath</c> is missing.
/// </exception>
public class CpuMonitorController(CpuMonitoringService service, ILogger<CpuMonitorController> logger, IConfiguration configuration) : Controller
{
    private readonly CpuMonitoringService _service = service;
    private readonly ILogger<CpuMonitorController> _logger = logger;

    /// <summary>
    /// Directory where CPU log files are stored.
    /// </summary>
    private readonly string _directoryPath = configuration["Metrics:DirectoryPath"]
          ?? throw new InvalidOperationException("Metrics:DirectoryPath is not configured.");

    /// <summary>
    /// Displays the CPU logs for the current day with pagination.
    /// </summary>
    /// <param name="currentPage">The page number to display. Defaults to the first page.</param>
    /// <param name="pageSize">The maximum number of log records displayed per page. Defaults to 50.</param>
    /// <returns>A view containing the paginated CPU logs.</returns>
    public async Task<IActionResult> Index(int currentPage = 1, int pageSize = 50)
    {
        // Load logs for the current day
        var logs = await LoadLogsAsync(DateTime.Today);

        var totalItems = logs.Count;

        currentPage = Math.Max(1, currentPage);
        pageSize = Math.Clamp(pageSize, 1, 500);

        logs.Reverse();

        logs = logs
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var model = new CpuLogViewModel
        {
            Logs = logs,
            IsRunning = _service.IsRunning,
            Pager = new PagerViewModel
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalItems = totalItems
            }
        };

        return View(model);
    }

    /// <summary>
    /// Starts CPU logging.
    /// </summary>
    /// <returns>Redirects to index page.</returns>
    public IActionResult Start()
    {
        bool started = _service.Start();

        TempData["status"] = started ? "Ok" : "Nok";
        TempData["message"] = started ? "CPU logging started" : "CPU logging is already running";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Stops CPU logging asynchronously.
    /// </summary>
    /// <returns>Redirects to index page.</returns>
    public async Task<IActionResult> Stop()
    {
        await _service.StopAsync();

        TempData["status"] = "Ok";
        TempData["message"] = "CPU logging stopped";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Loads CPU logs for a specific date from the log file.
    /// </summary>
    /// <param name="date">Date for which logs should be loaded.</param>
    /// <returns>List of parsed CPU log entries.</returns>
    private async Task<List<CpuLog>> LoadLogsAsync(DateTime date)
    {
        var result = new List<CpuLog>();
        var isoDate = date.ToString("yyyy-MM-dd");
        var logFile = Path.Combine(_directoryPath, $"{isoDate}.log");

        // If file does not exist, return empty result (no logs yet)
        if (!System.IO.File.Exists(logFile))
            return result;

        try
        {
            using var reader = new StreamReader(logFile);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var log = ParseLine(line);

                if (log != null)
                {
                    result.Add(log);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read CPU log file: {FilePath}", logFile);
        }

        return result;
    }

    /// <summary>
    /// Parses a single log line into a <see cref="CpuLog"/> instance.
    /// </summary>
    /// <param name="line">Raw log line in format: HH:mm:ss;CPU%</param>
    /// <returns>Parsed log or <c>null</c> if invalid.</returns>
    private CpuLog? ParseLine(string line)
    {
        var values = line.Split(';');

        if (values.Length != 2)
        {
            _logger.LogWarning("Invalid CPU log format: {Line}", line);
            return null;
        }

        if (!TimeOnly.TryParse(values[0], out var time))
        {
            _logger.LogWarning("Invalid time value in CPU log: {Line}", line);
            return null;
        }

        if (!double.TryParse(values[1], CultureInfo.InvariantCulture, out var cpu))
        {
            _logger.LogWarning("Invalid CPU value in log: {Line}", line);
            return null;
        }

        return new CpuLog
        {
            Time = time,
            CpuPercentage = cpu
        };
    }
}