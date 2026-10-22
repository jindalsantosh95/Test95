using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSSegment")]
    public class Segment
    {
        public string SegmentId { get; set; }
        public string AirCraft { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string ClassOfService { get; set; }
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public string EquipmentCode { get; set; }
        public string FareBasis { get; set; }
        public IEnumerable<Leg> Legs { get; set; }

        public static explicit operator Segment(TripMS.Segment segment)
        {
            return new()
            {
                SegmentId = $"0{segment.Id}",
                MarketingCarrier = segment.MarketingCarrier.CarrierCode,
                MarketingFlightNumber = segment.MarketingCarrier.FlightNumber,
                OperatingCarrier = segment.OperatingCarrier.CarrierCode,
                OperatingFlightNumber = segment.OperatingCarrier.FlightNumber,
                OriginAirportCode = segment.Origin.Code,
                DestinationAirportCode = segment.Destination.Code,
                ClassOfService = segment.Legs.FirstOrDefault()?.CabinClass.Code,
                ScheduledArrivalTime = new UtcAndLocalDateTime() { LocalDateTime = segment.ScheduledArrivalLocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                ScheduledDepartureTime = new UtcAndLocalDateTime() { LocalDateTime = segment.ScheduledDepartureLocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                FareBasis = segment.FareBasisCode,
                Legs = segment.Legs.Select(leg => (Leg)leg)
            };
        }
    }
}