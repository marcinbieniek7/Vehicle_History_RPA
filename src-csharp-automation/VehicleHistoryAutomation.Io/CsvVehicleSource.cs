
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Runtime.CompilerServices;
using VehicleHistoryAutomation.Core.Abstractions;
using VehicleHistoryAutomation.Core.Models;

namespace VehicleHistoryAutomation.Io;

public sealed class CsvVehicleSource : IVehicleSource
{
    private readonly string _filePath;
    private readonly ILogger<CsvVehicleSource> _logger;

    public CsvVehicleSource(string filePath, ILogger<CsvVehicleSource> logger)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));
        }

        _filePath = filePath;
        _logger = logger;
    }

    public string Description => $"CSV file: {_filePath}";

    public async IAsyncEnumerable<Vehicle> GetVehiclesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"Vehicles file not found: {_filePath}");
        }

        _logger.LogInformation("Reading vehicles from {Path}", _filePath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, config);

        await csv.ReadAsync();
        csv.ReadHeader();

        var rowNumber = 1;

        while (await csv.ReadAsync())
        {
            rowNumber++;
            cancellationToken.ThrowIfCancellationRequested();

            Vehicle? vehicle = null;

            try
            {
                var registration = csv.GetField<string>("Registration") ?? string.Empty;
                var vin = csv.GetField<string>("VIN") ?? string.Empty;
                var dateFirstRegistration = csv.GetField<string>("FirstRegistrationDate") ?? string.Empty;

                if (!DateOnly.TryParseExact(dateFirstRegistration, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    _logger.LogWarning("Row {Row}: invalid date format '{Date}', skipping this vehicle", rowNumber, dateFirstRegistration);
                    continue;
                }

                vehicle = new Vehicle(registration, vin, date);
            }
            catch (Exception ex) 
            {
                _logger.LogWarning(ex, "Row {Row}: failed to parse, skipping vehicle", rowNumber);
            }
            if (vehicle is not null)
            {
                yield return vehicle;
            }
        }
        _logger.LogInformation("FInished reading vehicles from {Path}", _filePath);
    }
}
