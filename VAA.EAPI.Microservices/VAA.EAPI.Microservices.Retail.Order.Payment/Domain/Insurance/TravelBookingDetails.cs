namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance
{  
    public class TravelBookingDetails
    {
        public string Currency { get; set; }
        public decimal TotalTripCost { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
    }
}