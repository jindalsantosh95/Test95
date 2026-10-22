using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class OrderItems
    {
        public string OrderItemNumber { get; set; }
        public string FlightIdRef { get; set; }
        public string SeatNumber { get; set; }
        public string ElectronicMiscellaneousDocumentNumber { get; set; }
        public string CustomerRefId { get; set; }
        public CurrencyAmount Amount { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
    }
}
