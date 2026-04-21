using VehicleHistoryAutomation.Core.Models;
namespace VehicleHistoryAutomation.Core.Abstractions;

public interface IVehicleSource
{
    string Description { get; }
    IAsyncEnumerable<Vehicle> GetVehiclesAsync(CancellationToken cancellationToken = default);
}
