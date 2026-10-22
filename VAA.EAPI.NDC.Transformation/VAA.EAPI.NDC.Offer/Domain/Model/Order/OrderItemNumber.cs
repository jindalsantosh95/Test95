
using System;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OrderItemNumber
    {
        public string OrderItemId { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }


    }
}
