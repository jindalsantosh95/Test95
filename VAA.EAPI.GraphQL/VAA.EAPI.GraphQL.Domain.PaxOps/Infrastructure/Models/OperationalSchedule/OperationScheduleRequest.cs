using NodaTime;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.OperationalSchedule
{
    public class OperationScheduleRequest
    {
        public string AirlineCode { get; set; }
        public int FlightNumber { get; set; }
        public LocalDateTime LocalDateFrom { get; set; }
        public LocalDateTime LocalDateTo { get; set; }
        public string Direction { get; set; }
        public int Limit { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }

        public static explicit operator OperationScheduleRequest(Segment segment)
        {
            return new OperationScheduleRequest
            {
                AirlineCode = segment.OperatingCarrier.CarrierCode ?? string.Empty,
                FlightNumber = int.Parse(segment.OperatingCarrier?.FlightNumber ?? "0"),
                LocalDateFrom = LocalDateTime.FromDateTime(segment.ScheduledDepartureLocalDateTime),
                LocalDateTo = LocalDateTime.FromDateTime(segment.ScheduledDepartureLocalDateTime),
                OriginAirportCode = segment.Origin?.Code ?? string.Empty,
                DestinationAirportCode = segment.Destination?.Code ?? string.Empty,
            };
        }
    }

}

