namespace Monitoring.Api.DTOs;

/// <summary>
/// DTO for responding with monitoring result details
/// </summary>
public class MonitoringResultDto
{
    public int Id { get; set; }

    public int MonitoringTargetId { get; set; }

    public int ResponseTime { get; set; }

    public int StatusCode { get; set; }

    public bool IsHealthy { get; set; }

    public DateTime CheckedAt { get; set; }

    public string? ErrorMessage { get; set; }
}