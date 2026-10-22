using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSSeatMap")]
    public class SeatMap
    {
        public string ClassType { get; set; }
        public string Equipment { get; set; }
        public string Location { get; set; }
        public string SeatOfferId { get; set; }
        public string SeatConfiguration { get; set; }
        public IEnumerable<SeatRow> SeatRows { get; set; }
        public IEnumerable<FareSummary> FareSummary { get; set; }
    }
}