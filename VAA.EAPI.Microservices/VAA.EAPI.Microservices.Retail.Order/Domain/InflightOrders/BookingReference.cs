using System;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders
{
    public class BookingReference
    {
        public string AirlineDesignator { get; set; }
        public string Rloc { get; set; }
        public DateTime? RlocCreateDate { get; set; }
    }
}
