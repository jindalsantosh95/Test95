namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OfferNumber
    {
        public string OfferId { get; set; }
        public string ShoppingId { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
        public Link Link { get; set; }
        public CurrencyAmount TotalPrice { get; set; }

    }
}
