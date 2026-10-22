using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripSeatMSSegmentLeg")]
    public class Leg
    {
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public string LegSequenceNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string OperatingCarrier { get; set; }
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
        public string DestinationAirportCode { get; set; }      

        public static explicit operator Leg(TripMS.Leg leg)
        {
            if (leg == null) return null;
            return new Leg()
            {
                DestinationAirportCode = leg.Destination.Code,
                OperatingCarrier = leg.Operating.CarrierCode,
                OriginAirportCode = leg.Origin.Code,                
                ScheduledArrivalTime = new UtcAndLocalDateTime() { LocalDateTime = leg.ScheduledArrivalLocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                ScheduledDepartureTime = new UtcAndLocalDateTime() { LocalDateTime = leg.ScheduledDepartureLocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
            };
        }

    }
}
