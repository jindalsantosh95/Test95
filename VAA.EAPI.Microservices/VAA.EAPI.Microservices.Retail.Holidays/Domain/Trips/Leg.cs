using Newtonsoft.Json.Linq;
using NodaTime;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Leg
    {
        public string Id { get; set; }
        public CarrierDetails Operating { get; set; }
        public CarrierDetails Marketing { get; set; }
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
        public AirportDetails Origin { get; set; }
        public AirportDetails Destination { get; set; }
        public string OnAirDuration { get; set; }
        public string Status { get; set; }
        public CabinClass CabinClass { get; set; }
        public Aircraft Aircraft { get; set; }
        public IEnumerable<SeatAssignment> SeatAssignments { get; set; }
        public string OnGroundDuration { get; set; }

        public static explicit operator Leg(JToken source)
            => FromSegment(source, source?["departure"]?.Value<string>(), source?["arrival"]?.Value<string>(), true, true);

        internal static Leg FromSegment(
            JToken source,
            string? origin,
            string? destination,
            bool isFirstLeg,
            bool isLastLeg,
            bool isMultiLeg = false)
        {
            if (source == null) return null;

            return new()
            {
                Operating = (CarrierDetails)(source["operatingAirline"]?.Value<string>(), source["flightNumber"]?.Value<string>()),
                Marketing = (CarrierDetails)source["marketingAirline"]?.Value<string>(),
                ScheduledDepartureLocal = isMultiLeg ? null : source["departureDateTime"]?.Value<DateTime>(),
                ScheduledArrivalLocal = isMultiLeg ? null : source["arrivalDateTime"]?.Value<DateTime>(),
                Origin = (AirportDetails)(origin, isFirstLeg ? source["depTerminal"]?.Value<string>() : null),
                Destination = (AirportDetails)(destination, isLastLeg ? source["desTerminal"]?.Value<string>() : null),
                Aircraft = (Aircraft)source,
            };
        }
    }
}
