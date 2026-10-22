namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance
{
    public class Purchase
    {
        public string PartnerBookingID { get; set; }
        public InsuranceBookingDetails InsuranceBookingDetails { get; set; }
        public TravelBookingDetails TravelBookingDetails { get; set; }
        public TravelDetails TravelDetails { get; set; }
        public ProductDetails ProductDetails { get; set; }
        public string Status { get; set; }
        public string OrderID { get; set; }
    }
}