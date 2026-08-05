using Microsoft.EntityFrameworkCore;
using Monitoring.Api.Data;
using Monitoring.Api.DTOs;
using Monitoring.Api.Models;

namespace Monitoring.Api.Services;

/// <summary>
/// Service for managing monitoring targets
/// Handles all business logic for CRUD operations
/// </summary>
public interface IMonitoringService
{
    Task<IEnumerable<MonitoringTargetResponseDto>> GetAllTargetsAsync();
    Task<MonitoringTargetResponseDto?> GetTargetByIdAsync(int id);
    Task<MonitoringTargetResponseDto> CreateTargetAsync(CreateMonitoringTargetDto dto);
    Task<MonitoringTargetResponseDto?> UpdateTargetAsync(int id, UpdateMonitoringTargetDto dto);
    Task<bool> DeleteTargetAsync(int id);
}

public class MonitoringService : IMonitoringService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MonitoringService> _logger;

    public MonitoringService(ApplicationDbContext dbContext, ILogger<MonitoringService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all monitoring targets from the database
    /// </summary>
    public async Task<IEnumerable<MonitoringTargetResponseDto>> GetAllTargetsAsync()
    {
        _logger.LogInformation("Retrieving all monitoring targets");

        var targets = await _dbContext.MonitoringTargets
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();

        var targetDtos = targets.Select(MapToResponseDto).ToList();
        _logger.LogInformation("Retrieved {Count} monitoring targets", targetDtos.Count);

        return targetDtos;
    }

    /// <summary>
    /// Retrieves a specific monitoring target by ID
    /// </summary>
    public async Task<MonitoringTargetResponseDto?> GetTargetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving monitoring target with ID {TargetId}", id);

        var target = await _dbContext.MonitoringTargets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (target == null)
        {
            _logger.LogWarning("Monitoring target with ID {TargetId} not found", id);
            return null;
        }

        return MapToResponseDto(target);
    }

    /// <summary>
    /// Creates a new monitoring target
    /// </summary>
    public async Task<MonitoringTargetResponseDto> CreateTargetAsync(CreateMonitoringTargetDto dto)
    {
        _logger.LogInformation("Creating new monitoring target: {TargetName} ({Url})", dto.Name, dto.Url);

        var target = new MonitoringTarget
        {
            Name = dto.Name,
            Url = dto.Url,
            MonitoringInterval = dto.MonitoringInterval,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.MonitoringTargets.Add(target);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully created monitoring target with ID {TargetId}", target.Id);

        return MapToResponseDto(target);
    }

    /// <summary>
    /// Updates an existing monitoring target
    /// </summary>
    public async Task<MonitoringTargetResponseDto?> UpdateTargetAsync(int id, UpdateMonitoringTargetDto dto)
    {
        _logger.LogInformation("Updating monitoring target with ID {TargetId}", id);

        var target = await _dbContext.MonitoringTargets
            .FirstOrDefaultAsync(t => t.Id == id);

        if (target == null)
        {
            _logger.LogWarning("Monitoring target with ID {TargetId} not found for update", id);
            return null;
        }

        // Only update fields that are provided (not null)
        if (!string.IsNullOrEmpty(dto.Name))
        {
            target.Name = dto.Name;
        }

        if (!string.IsNullOrEmpty(dto.Url))
        {
            target.Url = dto.Url;
        }

        if (dto.MonitoringInterval.HasValue)
        {
            target.MonitoringInterval = dto.MonitoringInterval.Value;
        }

        if (dto.IsActive.HasValue)
        {
            target.IsActive = dto.IsActive.Value;
        }

        target.UpdatedAt = DateTime.UtcNow;

        _dbContext.MonitoringTargets.Update(target);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully updated monitoring target with ID {TargetId}", id);

        return MapToResponseDto(target);
    }

    /// <summary>
    /// Deletes a monitoring target and all associated results
    /// </summary>
    public async Task<bool> DeleteTargetAsync(int id)
    {
        _logger.LogInformation("Deleting monitoring target with ID {TargetId}", id);

        var target = await _dbContext.MonitoringTargets
            .FirstOrDefaultAsync(t => t.Id == id);

        if (target == null)
        {
            _logger.LogWarning("Monitoring target with ID {TargetId} not found for deletion", id);
            return false;
        }

        _dbContext.MonitoringTargets.Remove(target);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted monitoring target with ID {TargetId}", id);

        return true;
    }

    /// <summary>
    /// Maps a MonitoringTarget entity to a response DTO
    /// </summary>
    private static MonitoringTargetResponseDto MapToResponseDto(MonitoringTarget target)
    {
        return new MonitoringTargetResponseDto
        {
            Id = target.Id,
            Name = target.Name,
            Url = target.Url,
            MonitoringInterval = target.MonitoringInterval,
            IsActive = target.IsActive,
            CreatedAt = target.CreatedAt,
            UpdatedAt = target.UpdatedAt
        };
    }
}
