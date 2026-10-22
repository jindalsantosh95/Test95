
using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    [GraphQLName("GraphqlSSRFlightLeg")]
    public class FlightLeg
    {
        public int? LegId { get; set; }
        public int? Ordinal { get; set; }
        public IEnumerable<Airline>? OperatingAirlines { get; set; }
        public string? FlightNumber { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
        public LocalDateTime? ScheduledDepartureLocal { get; set; }
        public LocalDateTime? ScheduledArrivalLocal { get; set; }

        public static explicit operator FlightLeg(Infrastructure.Models.SSRMS.FlightLeg source)
        {
            if (source == null)
                return null;

            return new()
            {
                FlightNumber = source.FlightNumber,
                LegId = source.LegId,
                OperatingAirlines = source.OperatingAirlines.Select(oa => (Airline)oa),
                Ordinal = source.Ordinal,
                ScheduledDepartureLocalDateTime = source.ScheduledDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = source.ScheduledArrivalLocalDateTime,
                ScheduledDepartureLocal = DateTimeHelper.ToLocal(source.ScheduledDepartureLocalDateTime),
                ScheduledArrivalLocal = DateTimeHelper.ToLocal(source.ScheduledArrivalLocalDateTime)
            };
        }
    }
}