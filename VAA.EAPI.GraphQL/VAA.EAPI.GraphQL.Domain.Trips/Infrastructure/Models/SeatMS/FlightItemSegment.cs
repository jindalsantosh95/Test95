using HotChocolate;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSFlightItemSegment")]
    public class FlightItemSegment
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
        public DateTime? ScheduledDepartureTime { get; set; }       
        public DateTime? ScheduledArrivalTime { get; set; }
        public string EquipmentCode { get; set; }

        public static explicit operator FlightItemSegment(TripMS.Segment segment)
        {           
            return new ()
            {
                SegmentId = $"0{segment.Id}",
                MarketingCarrier = segment.MarketingCarrier.CarrierCode,
                MarketingFlightNumber = segment.MarketingCarrier.FlightNumber,
                OperatingCarrier = segment.OperatingCarrier.CarrierCode,
                OperatingFlightNumber = segment.OperatingCarrier.FlightNumber,
                OriginAirportCode = segment.Origin.Code,
                DestinationAirportCode = segment.Destination.Code,
                ClassOfService = segment.Legs.FirstOrDefault()?.CabinClass.Code,
                ScheduledArrivalTime = segment.ScheduledArrivalLocalDateTime,
                ScheduledDepartureTime = segment.ScheduledDepartureLocalDateTime
            };
        }
    }
}