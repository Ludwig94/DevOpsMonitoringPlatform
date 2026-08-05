using System.ComponentModel.DataAnnotations;

namespace Monitoring.Api.DTOs;

/// <summary>
/// DTO for creating a new monitoring target
/// </summary>
public class CreateMonitoringTargetDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Url is required")]
    [Url(ErrorMessage = "Url must be a valid URL")]
    [StringLength(2048, MinimumLength = 5, ErrorMessage = "Url must be between 5 and 2048 characters")]
    public string Url { get; set; } = string.Empty;

    [Range(10, 3600, ErrorMessage = "MonitoringInterval must be between 10 and 3600 seconds")]
    public int MonitoringInterval { get; set; } = 60;
}
