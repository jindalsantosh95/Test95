namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class FlightOfferResponse
    {
        public Pos? Pos { get; set; }
        public Parties? Parties { get; set; }
        public string? ShoppingResponseId { get; set; }
        public int? TotalOffers { get; set; }
        public IEnumerable<OfferDetails> Offers { get; set; }
        public DataList DataList { get; set; }
        public bool IsAwardSearchResult => Offers.Any(offer => offer?.SalePrice?.FirstOrDefault(p => p.PriceComponentType == Price.PriceComponentTypes.PointsEnum)?.PointsAmount?.PointsCount > 0);
    }
}