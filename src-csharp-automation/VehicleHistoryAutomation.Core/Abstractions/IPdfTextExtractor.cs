namespace VehicleHistoryAutomation.Core.Abstractions;

public interface IPdfTextExtractor
{
    Task<string> ExtractTextAsync(Stream pdfContent, CancellationToken cancellationToken = default);
}
