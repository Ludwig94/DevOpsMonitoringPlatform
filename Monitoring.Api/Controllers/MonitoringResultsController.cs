using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monitoring.Api.Data;
using Monitoring.Api.DTOs;

namespace Monitoring.Api.Controllers;

/// <summary>
/// API endpoints for querying monitoring results and statistics
/// </summary>
[ApiController]
[Route("api/targets/{targetId}/results")]
public class MonitoringResultsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MonitoringResultsController> _logger;

    public MonitoringResultsController(
        ApplicationDbContext dbContext,
        ILogger<MonitoringResultsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get recent monitoring results for a specific target
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MonitoringResultDto>>> GetRecentResults(
        int targetId,
        [FromQuery] int limit = 50)
    {
        _logger.LogInformation(
            "HTTP GET: Retrieving recent results for target {TargetId}, limit: {Limit}",
            targetId,
            limit);

        if (targetId <= 0)
        {
            return BadRequest("Target ID must be greater than 0");
        }

        var targetExists = await _dbContext.MonitoringTargets
            .AnyAsync(t => t.Id == targetId);

        if (!targetExists)
        {
            _logger.LogWarning(
                "HTTP GET: Target {TargetId} not found",
                targetId);

            return NotFound(
                $"Monitoring target with ID {targetId} not found");
        }

        // Prevent excessively large requests.
        limit = Math.Min(Math.Max(limit, 1), 500);

        var results = await _dbContext.MonitoringResults
            .Where(r => r.MonitoringTargetId == targetId)
            .OrderByDescending(r => r.CheckedAt)
            .Take(limit)
            .Select(r => new MonitoringResultDto
            {
                Id = r.Id,
                MonitoringTargetId = r.MonitoringTargetId,
                ResponseTime = r.ResponseTime,
                StatusCode = r.StatusCode,
                IsHealthy = r.IsHealthy,
                CheckedAt = r.CheckedAt,
                ErrorMessage = r.ErrorMessage
            })
            .ToListAsync();

        _logger.LogInformation(
            "HTTP GET: Retrieved {Count} results for target {TargetId}",
            results.Count,
            targetId);

        return Ok(results);
    }

    /// <summary>
    /// Get uptime statistics for a specific target
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UptimeStatisticsDto>> GetUptimeStatistics(
        int targetId)
    {
        _logger.LogInformation(
            "HTTP GET: Retrieving uptime statistics for target {TargetId}",
            targetId);

        if (targetId <= 0)
        {
            return BadRequest("Target ID must be greater than 0");
        }

        var targetExists = await _dbContext.MonitoringTargets
            .AnyAsync(t => t.Id == targetId);

        if (!targetExists)
        {
            _logger.LogWarning(
                "HTTP GET: Target {TargetId} not found",
                targetId);

            return NotFound(
                $"Monitoring target with ID {targetId} not found");
        }

        var now = DateTime.UtcNow;

        var last24h = now.AddHours(-24);
        var last7d = now.AddDays(-7);
        var last30d = now.AddDays(-30);

        var allResults = await _dbContext.MonitoringResults
            .Where(r => r.MonitoringTargetId == targetId)
            .ToListAsync();

        var stats = new UptimeStatisticsDto
        {
            TargetId = targetId,
            CalculatedAt = now,

            Last24Hours = CalculateUptimePercentage(
                allResults
                    .Where(r => r.CheckedAt >= last24h)
                    .ToList()),

            Last7Days = CalculateUptimePercentage(
                allResults
                    .Where(r => r.CheckedAt >= last7d)
                    .ToList()),

            Last30Days = CalculateUptimePercentage(
                allResults
                    .Where(r => r.CheckedAt >= last30d)
                    .ToList()),

            AllTime = CalculateUptimePercentage(allResults)
        };

        _logger.LogInformation(
            "HTTP GET: Calculated statistics for target {TargetId} | " +
            "24h: {Uptime24h}% | " +
            "7d: {Uptime7d}% | " +
            "30d: {Uptime30d}%",
            targetId,
            stats.Last24Hours,
            stats.Last7Days,
            stats.Last30Days);

        return Ok(stats);
    }

    /// <summary>
    /// Get average response time for a specific target
    /// </summary>
    [HttpGet("average-response-time")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AverageResponseTimeDto>> GetAverageResponseTime(
        int targetId,
        [FromQuery] int hoursBack = 24)
    {
        _logger.LogInformation(
            "HTTP GET: Retrieving average response time for target {TargetId}, " +
            "last {Hours} hours",
            targetId,
            hoursBack);

        if (targetId <= 0)
        {
            return BadRequest("Target ID must be greater than 0");
        }

        if (hoursBack <= 0)
        {
            return BadRequest("Hours back must be greater than 0");
        }

        var targetExists = await _dbContext.MonitoringTargets
            .AnyAsync(t => t.Id == targetId);

        if (!targetExists)
        {
            _logger.LogWarning(
                "HTTP GET: Target {TargetId} not found",
                targetId);

            return NotFound(
                $"Monitoring target with ID {targetId} not found");
        }

        var cutoffTime = DateTime.UtcNow.AddHours(-hoursBack);

        var results = await _dbContext.MonitoringResults
            .Where(r =>
                r.MonitoringTargetId == targetId &&
                r.CheckedAt >= cutoffTime)
            .ToListAsync();

        var avgResponseTime = results.Any()
            ? (int)results.Average(r => r.ResponseTime)
            : 0;

        var dto = new AverageResponseTimeDto
        {
            TargetId = targetId,
            AverageResponseTimeMs = avgResponseTime,
            SampleCount = results.Count,
            PeriodHours = hoursBack,
            CalculatedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "HTTP GET: Average response time for target {TargetId} " +
            "is {AvgResponseTime}ms ({SampleCount} samples)",
            targetId,
            avgResponseTime,
            results.Count);

        return Ok(dto);
    }

    /// <summary>
    /// Calculates uptime percentage from a list of monitoring results.
    /// </summary>
    private static decimal CalculateUptimePercentage(
        List<Monitoring.Api.Models.MonitoringResult> results)
    {
        if (results.Count == 0)
        {
            return 0;
        }

        var healthyCount = results.Count(r => r.IsHealthy);

        return Math.Round(
            (decimal)healthyCount / results.Count * 100,
            2);
    }
}

/// <summary>
/// DTO for uptime statistics
/// </summary>
public class UptimeStatisticsDto
{
    public int TargetId { get; set; }

    public decimal Last24Hours { get; set; }

    public decimal Last7Days { get; set; }

    public decimal Last30Days { get; set; }

    public decimal AllTime { get; set; }

    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// DTO for average response time
/// </summary>
public class AverageResponseTimeDto
{
    public int TargetId { get; set; }

    public int AverageResponseTimeMs { get; set; }

    public int SampleCount { get; set; }

    public int PeriodHours { get; set; }

    public DateTime CalculatedAt { get; set; }
}