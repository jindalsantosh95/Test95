using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSOfferItem")]
    public class OfferItemNumber
    {
        public string OfferItemId { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
    }
}