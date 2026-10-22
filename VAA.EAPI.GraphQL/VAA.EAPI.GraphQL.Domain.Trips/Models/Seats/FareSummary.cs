using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("TripsSeatMapFareSummary")]
    
    public class FareSummary
    {
        public string SeatType { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
        public string Currency { get; set; }
        public string BrandId { get; set; }
        public string ProductId { get; set; }
    }
}