namespace CPU_Monitor.Models;

/// <summary>
/// View model used by the CPU logging page.
/// </summary>
public class CpuLogViewModel
{
    /// <summary>
    /// List of CPU log entries to display.
    /// </summary>
    public List<CpuLog> Logs { get; set; } = [];

    /// <summary>
    /// Indicates whether the logging service is currently running.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets the pagination information used to navigate through the CPU log entries.
    /// </summary>
    public PagerViewModel Pager { get; init; } = null!;
}