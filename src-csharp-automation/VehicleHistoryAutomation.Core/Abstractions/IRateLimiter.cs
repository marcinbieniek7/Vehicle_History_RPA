namespace VehicleHistoryAutomation.Core.Abstractions;

public interface IRateLimiter
{
    Task WaitAsync(CancellationToken cancellationToken = default);
}
