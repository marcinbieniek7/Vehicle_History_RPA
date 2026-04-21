using VehicleHistoryAutomation.Core.Models;

namespace VehicleHistoryAutomation.Core.Abstractions;

public interface IResultSink
{
    Task SaveAsync(InspectionData dasta, CancellationToken cancellationToken = default);
    Task FlushAsync(CancellationToken cancellationToken = default);
}
