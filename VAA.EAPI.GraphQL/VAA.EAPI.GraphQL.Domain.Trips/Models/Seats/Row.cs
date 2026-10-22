using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("TripsSeatMapRow")]
    public class Row
    {
        public int? RowNumber { get; set; }
        public bool OverWing { get; set; }
        public IEnumerable<Column> Columns { get; set; }
        public bool ExitLeft { get; set; }
        public bool ExitRight { get; set; }
    }
}