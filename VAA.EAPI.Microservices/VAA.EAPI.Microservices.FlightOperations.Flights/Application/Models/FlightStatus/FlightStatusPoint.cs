using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusPoint
    {
        public static explicit operator FlightStatusPoint?(DeltaDeparturePoint source)
        {
            var point = new FlightStatusPoint
            {
                AirportCode = source.Leg?.DepartureAirport?.Code,
                Terminal = source.Leg?.DepartureAirport?.Terminal.ToDisplayName(),
                Gate = source.Leg?.DepartureAirport?.Terminal?.Gate?.Id,
                ScheduledTime = source.Leg?.Status?.ScheduledDepartureTime,
                EstimatedTime = source.Leg?.Status?.EstimatedDepartureTime,
                ActualTime = source.Leg?.Status?.ActualDepartureTime
            };

            return HasAnyValue(point) ? point : null;
        }

        public static explicit operator FlightStatusPoint?(DeltaArrivalPoint source)
        {
            var point = new FlightStatusPoint
            {
                AirportCode = source.Leg?.ArrivalAirport?.Code,
                Terminal = source.Leg?.ArrivalAirport?.Terminal.ToDisplayName(),
                Gate = source.Leg?.ArrivalAirport?.Terminal?.Gate?.Id,
                ScheduledTime = source.Leg?.Status?.ScheduledArrivalTime,
                EstimatedTime = source.Leg?.Status?.EstimatedArrivalTime,
                ActualTime = source.Leg?.Status?.ActualArrivalTime
            };

            return HasAnyValue(point) ? point : null;
        }

        public string? AirportCode { get; set; }
        public string? Terminal { get; set; }
        public string? Gate { get; set; }
        public string? ScheduledTime { get; set; }
        public string? EstimatedTime { get; set; }
        public string? ActualTime { get; set; }

        private static bool HasAnyValue(FlightStatusPoint point) =>
            point.AirportCode is not null ||
            point.Terminal is not null ||
            point.Gate is not null ||
            point.ScheduledTime is not null ||
            point.EstimatedTime is not null ||
            point.ActualTime is not null;
    }
}

