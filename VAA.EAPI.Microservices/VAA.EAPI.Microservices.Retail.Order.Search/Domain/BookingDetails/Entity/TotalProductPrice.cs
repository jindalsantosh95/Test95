using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class TotalProductPrice
    {
        public string PriceComponentType { get; set; }
        public string PriceComponentName { get; set; }
        public string PriceComponentDescription { get; set; }
        public Amount Amount { get; set; }
    }
}
