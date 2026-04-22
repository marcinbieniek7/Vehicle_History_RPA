using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using VehicleHistoryAutomation.Core.Abstractions;
using VehicleHistoryAutomation.Core.Models;

namespace VehicleHistoryAutomation.Scraping;

public sealed class PlaywrightScraper
{
    private readonly ILogger<PlaywrightScraper> _logger;
    private readonly IRateLimiter _rateLimiter;
    private readonly IPdfStorage _pdfStorage;
    private readonly IPdfTextExtractor _pdfTextExtractor;
    private readonly InspectionPdfParser _parser;
    private ScraperOptions _options;

    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public PlaywrightScraper(
        ILogger<PlaywrightScraper> logger,
        IRateLimiter rateLimiter,
        IPdfStorage pdfStorage,
        IPdfTextExtractor pdfTextExtractor,
        InspectionPdfParser parser,
        IOptions<ScraperOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(rateLimiter);
        ArgumentNullException.ThrowIfNull(pdfStorage);
        ArgumentNullException.ThrowIfNull(pdfTextExtractor);
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _rateLimiter = rateLimiter;
        _pdfStorage = pdfStorage;
        _pdfTextExtractor = pdfTextExtractor;
        _parser = parser;
        _options = options.Value;
        )
    }

    public async Task<ScrapeResult> ScrapeAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        _logger.LogInformation("Starting scrape for vehicle {Registration} (VIN: {Vin}", vehicle.Registration, vehicle.Vin);
        
        await _rateLimiter.WaitAsync(cancellationToken);

        IBrowserContext? context = null;
        IPage? page = null;

        try
        {
            await EnsureBrowserStartedAsync();

        }
    }
}
