namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat
{
    public class SeatMap
    {
        public string SeatOfferId { get; set; }
        public string ClassType { get; set; }
        public string Equipment { get; set; }
        public IEnumerable<string> Details { get; set; }
        public IEnumerable<string> Configuration { get; set; }
        public IEnumerable<Row> Rows { get; set; }
        public IEnumerable<FareSummary> FareSummary { get; set; }
    }
}