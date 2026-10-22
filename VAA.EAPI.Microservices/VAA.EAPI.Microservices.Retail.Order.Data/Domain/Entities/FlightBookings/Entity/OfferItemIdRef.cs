

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class OfferItemIdRef
    {
        public string OrderItemId { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
    }
}
