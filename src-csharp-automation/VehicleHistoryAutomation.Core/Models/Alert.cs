namespace VehicleHistoryAutomation.Core.Models;

public sealed record Alert
{
    public Vehicle Vehicle { get; init; }
    public AlertType Type { get; init; }
    public string Message { get; init; }

    public Alert(Vehicle vehicle, AlertType type, string message)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        if (!Enum.IsDefined(type)) { throw new ArgumentException($"Invalid alert type: {type}", nameof(type)); }
        if (string.IsNullOrWhiteSpace(message)) { throw new ArgumentException("Message cannot be empty", nameof(message)); }

        Vehicle = vehicle;
        Type = type;
        Message = message;
    }

}
