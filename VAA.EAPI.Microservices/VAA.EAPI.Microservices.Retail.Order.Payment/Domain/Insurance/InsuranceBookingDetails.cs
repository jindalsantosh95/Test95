using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance
{
    public class InsuranceBookingDetails
    {
        public DateTime PurchaseDateTime { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public decimal TotalCoverageAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPremiumPrice { get; set; }
        public decimal WaiverPrice { get; set; }
        public decimal StreetRate { get; set; }
        public Purchaser Purchaser { get; set; }
        public string FulfillmentMethod { get; set; }
        public string PaymentMethod { get; set; }
    }
}