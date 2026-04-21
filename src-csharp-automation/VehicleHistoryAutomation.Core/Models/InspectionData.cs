namespace VehicleHistoryAutomation.Core.Models;

public sealed record InspectionData(
    Vehicle Vehicle,
    DateOnly? OcValidUntil,
    DateOnly? NextInspectionDate,
    DocumentStatus DocumentStatus,
    DateTime CheckedAt,
    PDFReference PDFReference);

