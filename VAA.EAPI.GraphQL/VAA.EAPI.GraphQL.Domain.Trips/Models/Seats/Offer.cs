using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("TripsSeatMapOffer")]
    public class Offer
    {
        public string OfferId { get; set; }
        public string OfferItemId { get; set; }
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string FareProduct { get; set; }
        public string FlightIdRef { get; set; }
        public SeatPrice Price { get; set; }
        public string FareProductName { get; set; }
    }
}