namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Payment
{
    public class PaymentRequest
    {
        public string TripReservationId { get; set; }
        public string IpAddress { get; set; }
        public string PaymentFailureLink { get; set; }
        public string PaymentSuccessLink { get; set; }
        public string? EmailAddress { get; set; }
    }
}
