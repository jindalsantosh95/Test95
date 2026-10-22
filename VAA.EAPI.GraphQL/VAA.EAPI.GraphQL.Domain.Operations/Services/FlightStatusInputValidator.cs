#nullable enable
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

namespace VAA.EAPI.GraphQL.Domain.Operations.Services;

internal static class FlightStatusInputValidator
{
    internal static void Validate(FlightStatusRouteRequest request, TimeProvider timeProvider)
    {
        var departure = request.DepartureAirportCode?.Trim();
        var arrival = request.ArrivalAirportCode?.Trim();
        if (!IsAirportCode(departure))
            throw new ArgumentException("Departure airport code must contain three letters.", nameof(request.DepartureAirportCode));
        if (!IsAirportCode(arrival))
            throw new ArgumentException("Arrival airport code must contain three letters.", nameof(request.ArrivalAirportCode));
        if (departure is not null && departure.Is(arrival))
            throw new ArgumentException("Departure and arrival airport codes must differ.", nameof(request.ArrivalAirportCode));

        ValidateDate(request.ScheduledDepartureDate, timeProvider, nameof(request.ScheduledDepartureDate));
        ValidateFilter(request.TimeRange, new[] { 1, 2, 3 }, nameof(request.TimeRange));
        ValidateFilter(request.Stops, new[] { 0, 1, 2 }, nameof(request.Stops));
    }

    internal static void Validate(FlightStatusNumberRequest request, TimeProvider timeProvider)
    {
        var number = request.FlightNumber?.Trim();
        if (number is null || number.IsNullOrWhiteSpace() || number.Length > 6 || !number.All(char.IsAsciiLetterOrDigit))
            throw new ArgumentException("Flight number must contain one to six letters or digits.", nameof(request.FlightNumber));

        ValidateDate(request.DepartureDate, timeProvider, nameof(request.DepartureDate));
        if (!IsAirportCode(request.DepartureAirportCode?.Trim()))
            throw new ArgumentException("Departure airport code must contain three letters.", nameof(request.DepartureAirportCode));
        if (request.SelectedLegId is not null &&
            (request.SelectedLegId.IsNullOrWhiteSpace() || request.SelectedLegId.Trim().Length > 32))
            throw new ArgumentException("Selected leg ID must contain one to 32 characters.", nameof(request.SelectedLegId));
    }

    private static void ValidateDate(DateOnly date, TimeProvider timeProvider, string parameterName)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (date == DateOnly.MinValue || date < today.AddDays(-1) || date > today.AddDays(331))
            throw new ArgumentException("Departure date must be between yesterday and 331 days from today.", parameterName);
    }

    private static bool IsAirportCode(string? value) =>
        value is { Length: 3 } && value.All(char.IsAsciiLetter);

    private static void ValidateFilter(IEnumerable<int>? values, IReadOnlyCollection<int> allowed, string parameterName)
    {
        if (values is null)
            return;

        var items = values.ToArray();
        if (items.Length is < 1 or > 3 || items.Distinct().Count() != items.Length || !items.All(allowed.Contains))
            throw new ArgumentException($"{parameterName} must contain one to three distinct allowed values.", parameterName);
    }
}
