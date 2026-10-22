using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance
{
    public class Product
    {
        public int Sequence { get; set; }
        public string QuoteID { get; set; }
        public string ProductID { get; set; }
        public InsuredDetails  InsuredDetails { get; set; }
        public decimal CoverageAmount { get; set; }
        public decimal Price { get; set; }
        public decimal PremiumPrice { get; set; }
        public decimal CharitableContribution { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProductBookingID { get; set; }
    }
}