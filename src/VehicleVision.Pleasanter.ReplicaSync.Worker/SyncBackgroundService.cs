using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;

namespace VehicleVision.Pleasanter.ReplicaSync.Worker;

/// <summary>
/// Background service that continuously runs sync operations.
/// </summary>
public class SyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncBackgroundService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory for creating DI scopes.</param>
    /// <param name="logger">The logger instance.</param>
    public SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReplicaSync Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var configRepo = scope.ServiceProvider.GetRequiredService<ISyncConfigRepository>();
                var syncEngine = scope.ServiceProvider.GetRequiredService<ISyncEngine>();

                var definitions = await configRepo.GetEnabledDefinitionsAsync(stoppingToken).ConfigureAwait(false);

                if (definitions.Count == 0)
                {
                    _logger.LogDebug("No enabled sync definitions found. Waiting...");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);
                    continue;
                }

                // Execute each sync definition
                foreach (var definition in definitions)
                {
                    _logger.LogInformation("Executing sync '{SyncId}'...", definition.SyncId);
                    var logEntry = await syncEngine.ExecuteSyncAsync(definition, stoppingToken).ConfigureAwait(false);
                    _logger.LogInformation(
                        "Sync '{SyncId}' completed: Status={Status}, Processed={Processed}, Inserted={Inserted}, Updated={Updated}, Deleted={Deleted}",
                        definition.SyncId, logEntry.Status, logEntry.RecordsProcessed,
                        logEntry.RecordsInserted, logEntry.RecordsUpdated, logEntry.RecordsDeleted);
                }

                // Wait for the minimum polling interval
                var minInterval = definitions.Min(d => d.PollingIntervalSeconds);
                await Task.Delay(TimeSpan.FromSeconds(minInterval), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in sync background service.");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("ReplicaSync Worker stopped.");
    }
}
