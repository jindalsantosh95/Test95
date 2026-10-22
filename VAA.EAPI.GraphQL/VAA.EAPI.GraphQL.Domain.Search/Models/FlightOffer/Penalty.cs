namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class Penalty
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public MaxPrice MaxPrice { get; set; }
        public MinPrice MinPrice { get; set; }
        public string Text { get; set; }
    }
}
