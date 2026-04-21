using VehicleHistoryAutomation.Core.Abstractions;

namespace VehicleHistoryAutomation.Scraping;

internal class TokenBucketRateLimiter : IRateLimiter
{
    private readonly TimeSpan _minimumInterval;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _gate = new(initialCount: 1, maxCount: 1);
    private DateTimeOffset _lastRequestTime = DateTimeOffset.MinValue;

    public TokenBucketRateLimiter(TimeSpan minimumInterval, TimeProvider? timeProvider = null)
    {
        if (minimumInterval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
            nameof(minimumInterval),
            "Minimum interval cannot be negative");
        }

        _minimumInterval = minimumInterval;
        _timeProvider = timeProvider ?? TimeProvider.System;

    }
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var now = _timeProvider.GetUtcNow();
            var timeSinceLastRequest = now - _lastRequestTime;

            if (timeSinceLastRequest < _minimumInterval)
            {
                var delay = _minimumInterval - timeSinceLastRequest;
                await Task.Delay(delay, _timeProvider, cancellationToken);
            }
            _lastRequestTime = _timeProvider.GetUtcNow();

        }
        finally { _gate.Release(); }
    }
}
