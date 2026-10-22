using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.MPI
{
    public class PaymentDetail
    {
        public string FormOfPayments { get; set; }

        public Amount Amount { get; set; }

        public string BspMethod { get; set; }

        public CardMethod CardMethod { get; set; }

        public PaymentPayer Payer { get; set; }

        public VoucherMethod VoucherMethod { get; set; }

        public PaymentCardAuthentication PaymentCardAuthentication { get; set; }
        


    }
}