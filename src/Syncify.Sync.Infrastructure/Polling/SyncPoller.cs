using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Infrastructure.Polling;

public sealed class SyncPoller : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncPoller> _logger;
    private readonly SyncPollerOptions _options;

    public SyncPoller(
        IServiceProvider serviceProvider,
        ILogger<SyncPoller> logger,
        IOptions<SyncPollerOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SyncPoller started with interval {Interval}s", _options.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollActiveRulesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "SyncPoller encountered an error during polling cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
        }
    }

    private async Task PollActiveRulesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var ruleRepository = scope.ServiceProvider.GetRequiredService<ISyncRuleRepository>();

        var activeRules = await ruleRepository.ListActiveAsync(ct);

        _logger.LogInformation("SyncPoller found {Count} active rules", activeRules.Count);

        foreach (var rule in activeRules)
        {
            if (ct.IsCancellationRequested)
                break;

            try
            { 
                // call usecase
                _logger.LogInformation("Polling rule {RuleId}", rule.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute sync for rule {RuleId}", rule.Id);
            }
        }
    }
}
