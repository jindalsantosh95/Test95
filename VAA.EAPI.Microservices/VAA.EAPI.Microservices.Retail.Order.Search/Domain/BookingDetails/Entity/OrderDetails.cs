using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class OrderDetails
    {
        public string OrderId { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
        public OrderChangeWebHook OrderChangeWebHook { get; set; }
        public string ValidatingCarrier { get; set; }
        public FarePrice FarePrice { get; set; }
        public List<object> StoredFare { get; set; }
        public List<object> FareQuote { get; set; }
        public CreationDateTime CreationDateTime { get; set; }
        public CancelledDateTime CancelledDateTime { get; set; }
        public AmendedDateTime AmendedDateTime { get; set; }
        public TotalFeesAndTaxes TotalFeesAndTaxes { get; set; }
        public TotalProductPrice TotalProductPrice { get; set; }
        public List<BookingReference> BookingReferences { get; set; }
    }
}