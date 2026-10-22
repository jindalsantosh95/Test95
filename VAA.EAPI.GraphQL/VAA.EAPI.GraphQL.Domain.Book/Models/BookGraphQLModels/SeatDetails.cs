using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels
{
    [GraphQLName("bookSummarySeatDetails")]
    public class SeatDetails
    {
        public string SectorId { get; set; }
        public string SeatNumber { get; set; }
        public string Pos { get; set; }
        public SeatPrice Price { get; set; }
    }
}