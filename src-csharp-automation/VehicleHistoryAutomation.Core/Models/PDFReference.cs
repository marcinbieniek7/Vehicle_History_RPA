namespace VehicleHistoryAutomation.Core.Models;

public sealed record PDFReference(
    string FilePath,
    string RelativePath,
    DateTime SavedAt);

