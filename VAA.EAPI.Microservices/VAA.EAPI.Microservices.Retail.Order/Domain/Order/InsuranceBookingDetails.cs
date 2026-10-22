using System;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class InsuranceBookingDetails
    {
        public DateTime PurchaseDateTime { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public double TotalCoverageAmount { get; set; }
        public double TotalPrice { get; set; }
        public double TotalPremiumPrice { get; set; }
        public double WaiverPrice { get; set; }
        public double StreetRate { get; set; }
        public Purchaser Purchaser { get; set; }
        public string FulfillmentMethod { get; set; }
        public string PaymentMethod { get; set; }
    }
}
