using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Segment
    {
        public string Id { get; set; }
        public string? PnrLocator { get; set; }
        public int? Ordinal { get; set; }
        public CarrierDetails OperatingCarrier { get; set; }
        public CarrierDetails MarketingCarrier { get; set; }
        public DateTime? ScheduledDepartureUtc { get; set; }
        public DateTime? ScheduledArrivalUtc { get; set; }
        public DateTime? EstimatedDepartureUtc { get; set; }
        public DateTime? EstimatedArrivalUtc { get; set; }
        public DateTime? ActualDepartureUtc { get; set; }
        public DateTime? ActualArrivalUtc { get; set; }
        public DateTime? ScheduledDepartureLocal { get; set; }
        public DateTime? ScheduledArrivalLocal { get; set; }
        public DateTime? ScheduledArrivalBasedOnDepartureLocal { get; set; }
        public DateTime? EstimatedDepartureLocal { get; set; }
        public DateTime? EstimatedArrivalLocal { get; set; }
        public DateTime? ActualDepartureLocal { get; set; }
        public DateTime? ActualArrivalLocal { get; set; }
        public string LayoverDuration { get; set; }
        public AirportDetails Origin { get; set; }
        public AirportDetails Destination { get; set; }
        public string ActionCode { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
        public string FlightSegmentId { get; set; }
        public AirlineDetails Operating { get; set; }
        public AirlineDetails Marketing { get; set; }
        public CabinClass BookedCabinClass { get; set; }
        public string CheckinStatus { get; set; }
        public string FareBasisCode { get; set; }
        public bool SeatEligible { get; set; }
        public OperationalStatus OperationalStatus { get; set; }
        public bool CheckedIn { get; set; }
        public bool? IsLayoverDurationValid { get; set; }

        public static explicit operator Segment(JToken source)
        {
            if (source == null) return null;

            return new()
            {
                PnrLocator = source["airlineLocator"]?.Value<string>(),
                OperatingCarrier = (CarrierDetails)(source["operatingAirline"]?.Value<string>(), source["flightNumber"]?.Value<string>()),
                MarketingCarrier = (CarrierDetails)source["marketingAirline"]?.Value<string>(),
                ScheduledDepartureLocal = source["departureDateTime"]?.Value<DateTime>(),
                ScheduledArrivalLocal = source["arrivalDateTime"]?.Value<DateTime>(),
                Origin = (AirportDetails)(source["departure"]?.Value<string>(), source["depTerminal"]?.Value<string>()),
                Destination = (AirportDetails)(source["arrival"]?.Value<string>(), source["desTerminal"]?.Value<string>()),
                Legs = CreateLegs(source)
            };
        }

        private static IEnumerable<Leg> CreateLegs(JToken source)
        {
            var departure = source["departure"]?.Value<string>();
            var arrival = source["arrival"]?.Value<string>();

            if (departure.IsNullOrWhiteSpace() || arrival.IsNullOrWhiteSpace())
            {
                return [(Leg)source];
            }

            var touchdownLocations = (source["touchdownDetails"] as JArray ?? [])
                .Select(touchdown => touchdown["location"]?.Value<string>())
                .Where(location => !location.IsNullOrWhiteSpace())
                .ToArray();

            var locations = new[] { departure }
                .Concat(touchdownLocations)
                .Append(arrival)
                .ToArray();

            if (locations.Length < 2)
            {
                return [(Leg)source];
            }

            return locations
                .Zip(locations.Skip(1), (origin, destination) => (origin, destination))
                .Select((route, index) => Leg.FromSegment(
                    source,
                    route.origin,
                    route.destination,
                    isFirstLeg: index == 0,
                    isLastLeg: index == locations.Length - 2,
                    isMultiLeg: touchdownLocations.Length > 0))
                .ToArray();
        }
    }
}
