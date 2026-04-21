using VehicleHistoryAutomation.Core.Models;

namespace VehicleHistoryAutomation.Core.Abstractions;

public interface IAlertNotifier
{
    Task NotifyAsync(Alert alert, CancellationToken cancellationToken = default);
}
