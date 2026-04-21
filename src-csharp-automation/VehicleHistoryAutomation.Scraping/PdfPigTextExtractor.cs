using System.Text;
using UglyToad.PdfPig;
using VehicleHistoryAutomation.Core.Abstractions;

namespace VehicleHistoryAutomation.Scraping;

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
	public Task<string> ExtractTextAsync(Stream pdfContent, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(pdfContent);
		cancellationToken.ThrowIfCancellationRequested();
		using var document = PdfDocument.Open(pdfContent);

		var textBuilder = new StringBuilder();

		foreach (var page in document.GetPages())
		{
			cancellationToken.ThrowIfCancellationRequested();
			textBuilder.AppendLine(page.Text);
		}
		return Task.FromResult(textBuilder.ToString());
	}
}
