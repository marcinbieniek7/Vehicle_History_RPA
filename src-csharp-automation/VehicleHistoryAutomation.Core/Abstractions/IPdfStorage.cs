using VehicleHistoryAutomation.Core.Models;

namespace VehicleHistoryAutomation.Core.Abstractions;

public interface IPdfStorage
{
    Task<PDFReference> SaveAsync(
        Vehicle vehicle,
        Stream pdfContent,
        CancellationToken cancellationToken = default);
}
