namespace VehicleHistoryAutomation.Scraping;

public sealed class ScraperOptions
{
    public string BaseUrl { get; init; } = "https://moj.gov.pl/nforms/engine/ng/index?xFormsAppName=HistoriaPojazdu";
    public bool Headless { get; init; } = false;
    public int NavigationTimeoutMs { get; init; } = 30_000;
    public int ElementTimeoutMs { get; init; } = 10_000;
    public int DownloadTimeout { get; init; } = 30_000;

}
