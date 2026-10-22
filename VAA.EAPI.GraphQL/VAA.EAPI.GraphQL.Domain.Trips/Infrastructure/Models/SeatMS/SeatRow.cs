using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSRow")]
    public class SeatRow
    {
        public string RowNumber { get; set; }
        public bool ExitOnLeft { get; set; }
        public bool ExitOnRight { get; set; }
        public bool Missing { get; set; }
        public bool OverWing { get; set; }
        public bool UpperDeck { get; set; }
        public IEnumerable<SeatColumn> SeatColumns { get; set; }
    }
}