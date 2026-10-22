namespace VAA.EAPI.Automation.MobileApp.UI.Models;

public sealed record FlightLegDetails(
    string Origin, string Destination, DateOnly DepartureDate, DateOnly ArrivalDate,
    TimeOnly DepartureTime, TimeOnly ArrivalTime, string OperatingCarrier, string? Descriptor);
