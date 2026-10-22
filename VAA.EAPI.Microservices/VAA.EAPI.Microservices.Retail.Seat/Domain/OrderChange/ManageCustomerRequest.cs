using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{
    public class ManageCustomerRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string Rloc { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<ManageCustomer> CustomerDetails { get; set; }
    }
}
