namespace VehicleHistoryAutomation.Core.Abstractions;

internal interface IPdfTextExtractor
{
    Task<string> ExtractTextAsync(Stream pdfContent, CancellationToken cancellationToken = default);
}
