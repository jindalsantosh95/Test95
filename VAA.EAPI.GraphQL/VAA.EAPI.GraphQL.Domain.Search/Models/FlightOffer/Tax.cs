namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class Tax
    {
        public string Code { get; set; }
        public CurrencyAmount Amount { get; set; }
        public string CountryCode { get; set; }
        public IEnumerable<CollectionPoint> CollectionPoints { get; set; }
        public bool Refundable { get; set; }
    }
}
