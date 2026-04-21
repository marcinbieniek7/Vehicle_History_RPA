using System.Globalization;
using System.Text.RegularExpressions;
using VehicleHistoryAutomation.Core.Models;


namespace VehicleHistoryAutomation.Scraping
{
	public sealed partial class InspectionPdfParser
	{
		private const string DateFormat = "dd.MM.yyyy";

		[GeneratedRegex(@"(?<=ważne do\s+)\d{2}\.\d{2}\.\d{4}", RegexOptions.RightToLeft)]
		private static partial Regex NextInspectionDateRegex();

		[GeneratedRegex(@"Data ważności polisy\s*(\d{2}\.\d{2}\.\d{4})")]
		private static partial Regex OcValidUntilRegex();

		[GeneratedRegex(@"Stan dokumentu\s+([a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ]+)")]
		private static partial Regex DocumentStatusRegex();

		public InspectionData Parse(string pdfText, Vehicle vehicle, PDFReference pdfReference)
		{
			ArgumentNullException.ThrowIfNull(pdfText);
			ArgumentNullException.ThrowIfNull(vehicle);
			ArgumentNullException.ThrowIfNull(pdfReference);

			var nextInspectionDate = ExtractDate(pdfText, NextInspectionDateRegex(), useFullMatch: true);
			var ocValidUntil = ExtractDate(pdfText, OcValidUntilRegex(), useFullMatch: false);
			var documentStatus = ExtractDocumentStatus(pdfText);

			return new InspectionData(
				Vehicle: vehicle,
				OcValidUntil: ocValidUntil,
				NextInspectionDate: nextInspectionDate,
				DocumentStatus: documentStatus,
				CheckedAt: DateTime.UtcNow,
				PDFReference: pdfReference);
		}

		private static DateOnly? ExtractDate(string text, Regex regex, bool useFullMatch)
		{
			var match = regex.Match(text);

			if (!match.Success)
			{
				return null;
			}

			var dateString = useFullMatch
				? match.Value
				: match.Groups[1].Value;

			if (DateOnly.TryParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
			{
				return date;
			}

			return null;
		}

		private static DocumentStatus ExtractDocumentStatus(string text)
		{
			var match = DocumentStatusRegex().Match(text);

			if (!match.Success)
			{
				return DocumentStatus.Unknown;
			}

			var statusWord = match.Groups[1].Value.Trim().ToLower();

			return statusWord switch
			{
				"odebrany" => DocumentStatus.Returned,
				"zatrzymany" => DocumentStatus.Seized,
				"zagubiony" => DocumentStatus.Lost,
				_ => DocumentStatus.Unknown
			};
		}
	}
}

