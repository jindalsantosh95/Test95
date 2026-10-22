namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class OfferIdReference
    {
        public string OfferId { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
        public string ShoppingResponseID { get; set; }
        public TotalPrice TotalPrice { get; set; }
        public Link Link { get; set; }
    }
}
