using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CPU_Monitor.Models;

/// <summary>
/// Represents a single CPU log entry.
/// </summary>
public class CpuLog
{
    /// <summary>
    /// Time of the measurement.
    /// </summary>
    [Key]
    [DisplayName("Time")]
    [DisplayFormat(DataFormatString = "{0:HH:mm:ss}", ApplyFormatInEditMode = true)]
    public TimeOnly Time { get; set; }

    /// <summary>
    /// CPU usage percentage.
    /// </summary>
    [DisplayName("CPU %")]
    public double CpuPercentage { get; set; }
}