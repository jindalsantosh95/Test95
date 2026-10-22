using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatContext")]
    public class Context
    {
        public string CurrencyCode {  get; set; }
        public string RecordLocator { get; set; }
    }
}