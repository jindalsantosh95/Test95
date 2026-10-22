namespace VAA.EAPI.GraphQL.Domain.FlightSearch.Models
{
    public class SearchRequest
    {
        public string? Airline_Code { get; set; }
        public string? Direction { get; set; }
        public int Limit { get; set; }
        public DateTime Local_Date_To { get; set; }
        public DateTime Local_Date_From { get; set; }
        public string? Origin_Airport_Code { get; set; }
    }

}
