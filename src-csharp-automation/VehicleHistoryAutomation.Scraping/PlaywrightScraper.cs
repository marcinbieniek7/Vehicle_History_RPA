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

            context = await _browser!.NewContextAsync(new BrowserNewContextOptions
            {
                AcceptDownloads = true
            });

            page = await context.NewPageAsync();
            page.SetDefaultNavigationTimeout(_options.ElementTimeoutMs);
            page.SetDefaultNavigationTimeout(_options.NavigationTimeoutMs);

            await NavigateToFormAsync(page, cancellationToken);
            await FillFormAsync(page, vehicle, cancellationToken);

            var submitResult = await SubmitResultAndCheckResultAsync(page, vehicle, cancellationToken);
            if (submitResult is not ScrapeResult.Success)
            {
                return submitResult;
            }

            var download = await DownloadPdfAsync(page, cancellationToken);

            await using var pdfStream = await download.CreateReadStreamAsync();
            var pdfReference = await _pdfStorage.SaveAsync(vehicle, pdfStream, cancellationToken);

            await using var pdfStreamForParsing = File.OpenRead(pdfReference.FilePath);
            var pdfText = await _pdfTextExtractor.ExtractTextAsync(pdfStreamForParsing, cancellationToken);
            var inspectionData = _parser.Parse(pdfText, vehicle, pdfReference);

            _logger.LogInformation("Successfully scraped vehicle {Registration}", vehicle.Registration);
            return new ScrapeResult.Success(inspectionData);



        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Timeout while scraping {Registration}", vehicle.Registration);
            return new ScrapeResult.Blocked();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scrape {Registration}", vehicle.Registration);
            return new ScrapeResult.Failed(ex.Message, ex);
        }
        finally
        {
            if (page != null)
            {
                await page.CloseAsync();
            }
            if (context != null)
            {
                await context.CloseAsync();
            }
        }
    }

    private async Task EnsureBrowserStartedAsync()
    {
        if (_browser != null)
        {
            return;
        }
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = _options.Headless
        });

    }

    private async Task NavigateToFormAsync(IPage page, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Navigating to {Url}", _options.BaseUrl);
        await page.GotoAsync(_options.BaseUrl);

        var openPageButton = page.Locator("text=OTWÓRZ STRONĘ");
        if (await openPageButton.IsVisibleAsync())
        {
            _logger.LogDebug("Landing page detected, clicking OTWÓRZ STRONĘ");
            await openPageButton.ClickAsync();
        }
        cancellationToken.ThrowIfCancellationRequested();

        await page.Locator("#registratoinNumber").WaitForAsync();
    }

    private async Task FillFormAsync(IPage page, Vehicle vehicle, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Filling form for vehicle {Registration}", vehicle.Registration);

        await page.Locator("#registrationNumber").FillAsync(vehicle.Registration);
        await page.Locator("#VINNumber").FillAsync(vehicle.Vin);
        await page.Locator("#firstRegistratoinDate").FillAsync(vehicle.FirstRegistrationDate.ToString("dd.MM.yyyy"));
    }

    private async Task<ScrapeResult> SubmitResultAndCheckResultAsync(IPage page, Vehicle vehicle, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug("Submitting form for vehicle {Registration}", vehicle.Registration);
        await page.Locator("button:has-text('Sprawdź pojazd')").ClickAsync();

        var downloadButton = page.Locator("button:has-text('Pobierz  raport PDF')");
        var notFoundMessage = page.Locator("text=Nie znaleźliśmy pojazdu");

        var winner = await Task.WhenAny(
        downloadButton.WaitForAsync().ContinueWith(_ => "download", cancellationToken),
        notFoundMessage.WaitForAsync().ContinueWith(_ => "notfound", cancellationToken)
        );

        var result = await winner;

        if (result == "notfound")
        {
            _logger.LogInformation("Vehicle {Registration} not found in CEP Database", vehicle.Registration);
            return new ScrapeResult.NotFound();
        }

        return new ScrapeResult.Success(null!);
    }

    private async Task<IDownload> DownloadPdfAsync(IPage page, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug("Downloading PDF");

        var downloadTask = page.WaitForDownloadAsync(new PageWaitForDownloadOptions
        {
            Timeout = _options.DownloadTimeout
        });

        await page.Locator("button:has-text('Pobierz  raport PDF;").ClickAsync();

        return await downloadTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.DisposeAsync();
            _browser = null;
        }
        _playwright?.Dispose();
        _playwright = null;
    }
}
