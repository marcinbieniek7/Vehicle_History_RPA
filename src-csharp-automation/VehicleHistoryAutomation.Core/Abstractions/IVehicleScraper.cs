using VehicleHistoryAutomation.Core.Models;

namespace VehicleHistoryAutomation.Core.Abstractions;

public interface IVehicleScraper
{
    Task<ScrapeResult> ScrapeAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
}
