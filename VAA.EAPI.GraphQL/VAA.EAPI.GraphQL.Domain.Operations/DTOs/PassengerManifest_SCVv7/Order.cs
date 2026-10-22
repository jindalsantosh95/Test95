namespace VAA.EAPI.GraphQL.Domain.Operations.DTOs.PassengerManifest_SCVv7
{
    public class Order
    {
        public string PNRRefNumber { get; set; }
        public DateTime PNRCreationDate { get; set; }
        public string BookingID { get; set; }
        public OrderItem[] OrderItems { get; set; }
        public object Links { get; set; }
    }
}
