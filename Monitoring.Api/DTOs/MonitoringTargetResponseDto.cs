namespace Monitoring.Api.DTOs;

/// <summary>
/// DTO for responding with monitoring target details
/// </summary>
public class MonitoringTargetResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int MonitoringInterval { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
