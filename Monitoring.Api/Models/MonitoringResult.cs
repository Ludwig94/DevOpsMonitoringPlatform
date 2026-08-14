using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring.Api.Models;

/// <summary>
/// Represents a monitoring check result for a target.
/// </summary>
public class MonitoringResult
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(MonitoringTarget))]
    [Required]
    public int MonitoringTargetId { get; set; }

    /// <summary>
    /// Response time in milliseconds.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int ResponseTime { get; set; }

    /// <summary>
    /// HTTP status code.
    /// </summary>
    [Range(0, 599)]
    public int StatusCode { get; set; }

    /// <summary>
    /// Whether the target was healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// When the check was performed.
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Error message if the check failed.
    /// </summary>
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Navigation property to the monitoring target.
    /// </summary>
    public MonitoringTarget MonitoringTarget { get; set; } = null!;
}