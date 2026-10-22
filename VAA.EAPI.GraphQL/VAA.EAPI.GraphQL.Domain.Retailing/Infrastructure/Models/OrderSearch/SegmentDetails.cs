using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch
{
    [GraphQLName("Retailing.SegmentDetails")]
    public class SegmentDetails
    {        
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }       
        public string OriginAirportCode { get; set; }       
        public string DestinationAirportCode { get; set; }      
        public string ClassOfService { get; set; }
        public ScheduledTime ScheduledArrivalTime { get; set; }
        public ScheduledTime ScheduledDepartureTime { get; set; }     
        public string SegmentNumber { get; set; }
        public string SegmentId { get; set; }        
        public CabinType CabinType { get; set; }       
    }
}