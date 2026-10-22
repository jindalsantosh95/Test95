using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusNumberResponse
    {
        public static explicit operator FlightStatusNumberResponse(DeltaFlightDetailsResponse source)
        {
            var response = new FlightStatusNumberResponse
            {
                SelectedLegId = source.SelectedLegId?.Trim(),
                FlightDate = source.FlightDate,
                FlightNumber = source.FlightNumber
            };

            var legs = source.Segment?.Legs?
                .Where(leg => leg is not null)
                .Select(leg => (FlightStatusLeg)leg!)
                .Where(HasAnyValue)
                .ToList() ?? [];
            response.Legs = legs;
            var selectedLegId = source.SelectedLegId?.Trim();
            response.ActiveLeg = !selectedLegId.IsNullOrWhiteSpace()
                ? legs.FirstOrDefault(x => x.Id.Is(selectedLegId)) ?? legs.FirstOrDefault()
                : legs.FirstOrDefault();

            return response;
        }

        public string? SelectedLegId { get; set; }
        public string? FlightDate { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public IEnumerable<FlightStatusLeg> Legs { get; set; } = Enumerable.Empty<FlightStatusLeg>();
        public FlightStatusLeg? ActiveLeg { get; set; }
        public IEnumerable<FlightStatusWarning> Warnings { get; set; } = Array.Empty<FlightStatusWarning>();

        private static bool HasAnyValue(FlightStatusLeg leg) =>
            leg.Id is not null ||
            leg.MarketingFlightNumber is not null ||
            leg.Departure is not null ||
            leg.Arrival is not null ||
            leg.Aircraft is not null ||
            leg.Status is not null;
    }
}
