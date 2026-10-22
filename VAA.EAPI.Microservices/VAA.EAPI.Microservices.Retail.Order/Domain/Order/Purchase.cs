using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class Purchase
    {
        public string PartnerBookingID { get; set; }
        public InsuranceBookingDetails InsuranceBookingDetails { get; set; }
        public TravelBookingDetails TravelBookingDetails { get; set; }
        public TravelDetails TravelDetails { get; set; }
        public Providers Providers { get; set; }
        public ProductDetails ProductDetails { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public string CxId { get; set; }
        public string ResponseId { get; set; }
        public string SupplierCode { get; set; }
    }
}