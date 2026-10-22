using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Seats
{
    public class SeatMap
    {
        public string ClassType { get; set; }
        public string Equipment { get; set; }
        public string Location { get; set; }
        public string SeatConfiguration { get; set; }
        public IEnumerable<SeatRow> SeatRows { get; set; }
        public IEnumerable<FareSummary> FareSummary { get; set; }
    }
}
