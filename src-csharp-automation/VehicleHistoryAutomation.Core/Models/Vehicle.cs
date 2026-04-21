namespace VehicleHistoryAutomation.Core.Models;

public sealed record Vehicle
{
    public string Registration { get; init; }
    public string Vin { get; init; }
    public DateOnly FirstRegistrationDate { get; init; }

    public Vehicle(string registration, string vin, DateOnly firstRegistrationDate)
    {
        if (string.IsNullOrWhiteSpace(registration))
        { throw new ArgumentException("Registration cannot be empty.", nameof(registration)); }
        if (string.IsNullOrWhiteSpace(vin))
        { throw new ArgumentException("Vin cannot be empty.", nameof(vin)); }
        if (vin.Length != 17)
        { throw new ArgumentException($"Vin must be exactly 17 characters, got {vin.Length}", nameof(vin)); }
        if (firstRegistrationDate > DateOnly.FromDateTime(DateTime.Today))
        { throw new ArgumentException("First registration date cannot be in the future", nameof(firstRegistrationDate)); }

        Registration = registration;
        Vin = vin;
        FirstRegistrationDate = firstRegistrationDate;


    }

}

