using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.MPI
{
    public class PaymentRequest
    {
        public Pos Pos { get; set; }

        public Parties Parties { get; set; }

        public BookingReferences BookingReferences { get; set; }

        public IEnumerable<PaymentDetail> PaymentDetails { get; set; }
    }
}
