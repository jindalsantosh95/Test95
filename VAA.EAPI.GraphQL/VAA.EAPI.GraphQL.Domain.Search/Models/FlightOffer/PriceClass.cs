namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class PriceClass
    {
        public string? Id { get; set; }
        public string? FareBasisCode { get; set; }
        public string? FareTypeCode { get; set; }
        public string? ProductName { get; set; }
        public string? ClassOfService { get; set; }
        public CabinType? CabinType { get; set; }
    }
}