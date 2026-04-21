namespace VehicleHistoryAutomation.Core.Models;

public abstract record ScrapeResult
{
    public sealed record Success(InspectionData Data) : ScrapeResult;
    public sealed record NotFound : ScrapeResult;
    public sealed record Blocked : ScrapeResult;
    public sealed record Failed(string Reason, Exception? Exception = null) : ScrapeResult;
}
