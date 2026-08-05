using Microsoft.AspNetCore.Mvc;
using Monitoring.Api.DTOs;
using Monitoring.Api.Services;

namespace Monitoring.Api.Controllers;

/// <summary>
/// API endpoints for managing monitoring targets
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MonitoringTargetsController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;
    private readonly ILogger<MonitoringTargetsController> _logger;

    public MonitoringTargetsController(IMonitoringService monitoringService, ILogger<MonitoringTargetsController> logger)
    {
        _monitoringService = monitoringService;
        _logger = logger;
    }

    /// <summary>
    /// Get all monitoring targets
    /// </summary>
    /// <returns>List of all monitoring targets</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MonitoringTargetResponseDto>>> GetAll()
    {
        _logger.LogInformation("HTTP GET: Retrieving all monitoring targets");
        var targets = await _monitoringService.GetAllTargetsAsync();
        return Ok(targets);
    }

    /// <summary>
    /// Get a specific monitoring target by ID
    /// </summary>
    /// <param name="id">The ID of the monitoring target</param>
    /// <returns>The monitoring target details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MonitoringTargetResponseDto>> GetById(int id)
    {
        _logger.LogInformation("HTTP GET: Retrieving monitoring target with ID {TargetId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("HTTP GET: Invalid target ID {TargetId}", id);
            return BadRequest("Target ID must be greater than 0");
        }

        var target = await _monitoringService.GetTargetByIdAsync(id);

        if (target == null)
        {
            _logger.LogWarning("HTTP GET: Monitoring target with ID {TargetId} not found", id);
            return NotFound($"Monitoring target with ID {id} not found");
        }

        return Ok(target);
    }

    /// <summary>
    /// Create a new monitoring target
    /// </summary>
    /// <param name="createDto">The monitoring target details to create</param>
    /// <returns>The created monitoring target</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MonitoringTargetResponseDto>> Create([FromBody] CreateMonitoringTargetDto createDto)
    {
        _logger.LogInformation("HTTP POST: Creating new monitoring target: {TargetName}", createDto.Name);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("HTTP POST: Invalid model state for new monitoring target");
            return BadRequest(ModelState);
        }

        var createdTarget = await _monitoringService.CreateTargetAsync(createDto);
        return CreatedAtAction(nameof(GetById), new { id = createdTarget.Id }, createdTarget);
    }

    /// <summary>
    /// Update an existing monitoring target
    /// </summary>
    /// <param name="id">The ID of the monitoring target to update</param>
    /// <param name="updateDto">The updated monitoring target details</param>
    /// <returns>The updated monitoring target</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MonitoringTargetResponseDto>> Update(int id, [FromBody] UpdateMonitoringTargetDto updateDto)
    {
        _logger.LogInformation("HTTP PUT: Updating monitoring target with ID {TargetId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("HTTP PUT: Invalid target ID {TargetId}", id);
            return BadRequest("Target ID must be greater than 0");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("HTTP PUT: Invalid model state for monitoring target update");
            return BadRequest(ModelState);
        }

        var updatedTarget = await _monitoringService.UpdateTargetAsync(id, updateDto);

        if (updatedTarget == null)
        {
            _logger.LogWarning("HTTP PUT: Monitoring target with ID {TargetId} not found", id);
            return NotFound($"Monitoring target with ID {id} not found");
        }

        return Ok(updatedTarget);
    }

    /// <summary>
    /// Delete a monitoring target
    /// </summary>
    /// <param name="id">The ID of the monitoring target to delete</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("HTTP DELETE: Deleting monitoring target with ID {TargetId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("HTTP DELETE: Invalid target ID {TargetId}", id);
            return BadRequest("Target ID must be greater than 0");
        }

        var deleted = await _monitoringService.DeleteTargetAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("HTTP DELETE: Monitoring target with ID {TargetId} not found", id);
            return NotFound($"Monitoring target with ID {id} not found");
        }

        return NoContent();
    }
}
