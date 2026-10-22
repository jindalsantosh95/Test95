namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class TravelBookingDetails
    {
        public string Currency { get; set; }
        public double TotalTripCost { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
    }
}