using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels
{
    [GraphQLName("bookSummaryPassengerSeatMap")]
    public class FlightPassengerSeatMap
    {
        public string PassengerId { get; set; }
        public string PassengerFirstName { get; set; }
        public string PassengerLastName { get; set; }
        public IEnumerable<SeatDetails> SeatDetails { get; set; }
        public LinkedInfantDetails LinkedInfant { get; set; }
    }
}