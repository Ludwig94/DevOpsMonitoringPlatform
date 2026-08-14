using Monitoring.Api.Data;
using Monitoring.Api.Models;
using Monitoring.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Monitoring.Api.BackgroundServices;

/// <summary>
/// Background service that periodically performs health checks on active monitoring targets.
/// Optimized to reduce unnecessary Azure SQL Database compute usage.
/// </summary>
public class HealthCheckHostedService : BackgroundService
{
    private readonly ILogger<HealthCheckHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    // Check the database for targets that need monitoring once per minute.
    private const int SchedulerIntervalSeconds = 60;

    // Prevent too many external requests from running simultaneously.
    private const int MaxConcurrentChecks = 5;

    // Track the next check time for each target.
    private readonly Dictionary<int, DateTime> _nextCheckTimes = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HealthCheckHostedService is starting");

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(SchedulerIntervalSeconds));

        try
        {
            // Perform an initial check immediately after startup.
            await PerformScheduledChecksAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PerformScheduledChecksAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "HealthCheckHostedService cancellation requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Fatal error occurred in HealthCheckHostedService");
        }
        finally
        {
            _logger.LogInformation(
                "HealthCheckHostedService has stopped");
        }
    }

    /// <summary>
    /// Finds targets that are due and performs their health checks.
    /// </summary>
    private async Task PerformScheduledChecksAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var healthCheckService =
            scope.ServiceProvider.GetRequiredService<IHealthCheckService>();

        // One database query per scheduler interval instead of every 5 seconds.
        var activeTargets = await dbContext.MonitoringTargets
            .Where(t => t.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var activeTargetIds = activeTargets
            .Select(t => t.Id)
            .ToHashSet();

        // Remove targets that are no longer active.
        var inactiveTargetIds = _nextCheckTimes.Keys
            .Where(id => !activeTargetIds.Contains(id))
            .ToList();

        foreach (var targetId in inactiveTargetIds)
        {
            _nextCheckTimes.Remove(targetId);

            _logger.LogDebug(
                "Removed inactive target ID {TargetId} from scheduler",
                targetId);
        }

        // Find targets that need to be checked.
        var checksToPerform = activeTargets
            .Where(target =>
                !_nextCheckTimes.TryGetValue(target.Id, out var nextCheck) ||
                now >= nextCheck)
            .ToList();

        if (checksToPerform.Count == 0)
        {
            return;
        }

        _logger.LogDebug(
            "Performing {CheckCount} scheduled health checks",
            checksToPerform.Count);

        // Limit the number of simultaneous health checks.
        using var semaphore = new SemaphoreSlim(MaxConcurrentChecks);

        var checkTasks = checksToPerform.Select(async target =>
        {
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                await PerformHealthCheckAsync(
                    target,
                    healthCheckService,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to perform health check for target {TargetName} (ID: {TargetId})",
                    target.Name,
                    target.Id);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(checkTasks);
    }

    /// <summary>
    /// Performs a health check and saves the result.
    /// </summary>
    private async Task PerformHealthCheckAsync(
        MonitoringTarget target,
        IHealthCheckService healthCheckService,
        CancellationToken cancellationToken)
    {
        var result = await healthCheckService.CheckHealthAsync(
            target.Url,
            cancellationToken);

        var monitoringResult = new MonitoringResult
        {
            MonitoringTargetId = target.Id,
            ResponseTime = result.ResponseTimeMs,
            StatusCode = result.StatusCode,
            IsHealthy = result.IsHealthy,
            CheckedAt = DateTimeOffset.UtcNow,
            ErrorMessage = result.ErrorMessage
        };

        // Use a separate scope/context for the database write.
        using var saveScope = _serviceProvider.CreateScope();

        var savingDbContext =
            saveScope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        savingDbContext.MonitoringResults.Add(monitoringResult);

        await savingDbContext.SaveChangesAsync(cancellationToken);

        // Schedule the next check based on the target's configured interval.
        _nextCheckTimes[target.Id] =
            DateTime.UtcNow.AddSeconds(target.MonitoringInterval);

        _logger.LogDebug(
            "Health check completed for {TargetName} (ID: {TargetId}) | " +
            "Healthy: {IsHealthy} | NextCheck: {NextCheckTime}",
            target.Name,
            target.Id,
            result.IsHealthy,
            _nextCheckTimes[target.Id]);
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "HealthCheckHostedService is stopping");

        await base.StopAsync(cancellationToken);
    }
}
