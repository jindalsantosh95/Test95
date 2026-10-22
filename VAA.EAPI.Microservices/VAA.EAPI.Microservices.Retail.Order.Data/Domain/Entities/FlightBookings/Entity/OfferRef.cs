

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class OfferRef
    {
        public string OrderId { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
        public Link Link { get; set; }
    }
}
