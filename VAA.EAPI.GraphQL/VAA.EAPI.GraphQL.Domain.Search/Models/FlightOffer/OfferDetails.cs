namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class OfferDetails
    {
        public string? OfferId { get; set; }
        public string? RecordLocatorId { get; set; }
        public AveragePassengerPrice? AveragePassengerPrice { get; set; }
        public HighestPassengerPrice? HighestPassengerPrice { get; set; }
        public AddCollectPrice? AddCollectPrice { get; set; }
        public IEnumerable<Price>? SalePrice { get; set; }
        public IEnumerable<Price>? SalePriceDifferences { get; set; }
        public bool Selected { get; set; }
        public bool PriceChanged { get; set; }
        public bool UpsellOffer { get; set; }
        public bool Refundable { get; set; }
        public string PromoCodeApplied { get; set; }
        public bool? IsSaverFare { get; set; }
        public IEnumerable<OfferItem> OfferItems { get; set; }
        public string OfferExpirationTimeLimit { get; set; }
        public string? TicketingTimeLimit { get; set; }
        public string? Availability { get; set; }
        public int? AvailableSeatCount { get; set; }
    }
}