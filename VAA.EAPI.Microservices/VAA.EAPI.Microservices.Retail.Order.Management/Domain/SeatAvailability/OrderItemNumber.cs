using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability
{
    public class OrderItems
    {
        public string OrderItemNumber { get; set; }
        public string FlightIdRef { get; set; }
        public string SeatNumber { get; set; }
        public string ElectronicMiscellaneousDocumentNumber { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }

    }
}
