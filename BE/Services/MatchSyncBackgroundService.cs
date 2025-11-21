using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BE.Services;

public class MatchSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MatchSyncBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(4);

    public MatchSyncBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<MatchSyncBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Match Sync Background Service running.");

        using (var timer = new PeriodicTimer(_period))
        {
            // Run immediately on startup
            await DoWorkAsync();

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync();
            }
        }
    }

    private async Task DoWorkAsync()
    {
        try
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var matchSyncService = scope.ServiceProvider.GetRequiredService<MatchSyncService>();
                await matchSyncService.SyncMatchesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing Match Sync Background Service.");
        }
    }
}
