using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCSeatUpdate
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
        public string SeatAssignmentType {  get; set; }
    }
}
