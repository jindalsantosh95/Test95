using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class FlightLeg
    {
        public int LegId { get; set; }
        public int Ordinal { get; set; }
        public IEnumerable<Airline> OperatingAirlines { get; set; }
        public string FlightNumber { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }

        public static explicit operator FlightLeg(Leg source)
        {
            if (source == null)
                return null;

            return new FlightLeg
            {
                LegId = source.LegId,
                Ordinal = source.Ordinal,
                OperatingAirlines = source.Carrier?.Select(c => (Airline)c) ?? Enumerable.Empty<Airline>(),
                FlightNumber = source.FlightNum,
                ScheduledDepartureLocalDateTime = source.TransportOrigin?.ScheduledDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = source.TransportDestination?.ScheduledArrivalLocalDateTime
            };
        }
    }
}
