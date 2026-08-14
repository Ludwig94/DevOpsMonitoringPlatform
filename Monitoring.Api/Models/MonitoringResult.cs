using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitoring.Api.Models;

/// <summary>
/// Represents a monitoring check result for a target
/// </summary>
public class MonitoringResult
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(MonitoringTarget))]
    [Required]
    public int MonitoringTargetId { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "ResponseTime must be a non-negative number")]
    public int ResponseTime { get; set; }

    /// <summary>
    /// HTTP status code (e.g., 200, 404, 500)
    /// </summary>
    [Range(0, 599, ErrorMessage = "StatusCode must be between 0 and 599")]
    public int StatusCode { get; set; }

    /// <summary>
    /// Whether the target was healthy (status code 200-299)
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// When the check was performed
    /// </summary>
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Error message if the check failed
    /// </summary>
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    // Navigation property
    public MonitoringTarget MonitoringTarget { get; set; } = null!;
}
