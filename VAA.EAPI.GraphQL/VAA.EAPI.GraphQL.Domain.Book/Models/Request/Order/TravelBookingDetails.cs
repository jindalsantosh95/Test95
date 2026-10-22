namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    public class TravelBookingDetails
    {
        public string Currency { get; set; }
        public double TotalTripCost { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
    }
}