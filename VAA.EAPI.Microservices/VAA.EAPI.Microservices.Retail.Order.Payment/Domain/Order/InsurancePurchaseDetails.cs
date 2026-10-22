using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Order
{
    public class InsurancePurchaseDetails
    {
        public InsurancePurchaseStatus InsurancePurchaseStatus { get; set; }
        public decimal? InsurancePurchaseAmount { get; set; }
        public string TransactionId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}