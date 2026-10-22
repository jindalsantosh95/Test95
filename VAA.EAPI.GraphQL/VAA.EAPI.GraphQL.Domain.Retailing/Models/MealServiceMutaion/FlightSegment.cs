using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Models.MealServiceMutaion
{
    [GraphQLName("MealServiceFlightSegment")]    
    public class FlightSegment
    {
        public DateTime FlightDate { get; set; }
        public string SegmentId { get; set; }
        public FlightNumber FlightNumber { get; set; }        
        public IEnumerable<Passenger> Passengers { get; set; }
    }
}
