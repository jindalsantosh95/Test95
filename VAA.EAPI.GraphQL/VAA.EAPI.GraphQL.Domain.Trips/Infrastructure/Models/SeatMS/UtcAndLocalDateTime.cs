using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSDateTime")]
    public class UtcAndLocalDateTime
    {
        public string UtcDateTime { get; set; }
        public string LocalDateTime { get; set; }
    }
}