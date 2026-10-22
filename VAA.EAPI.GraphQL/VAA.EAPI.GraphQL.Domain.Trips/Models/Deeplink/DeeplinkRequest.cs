namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Deeplink
{
    public class DeeplinkRequest
    {
        public string TripReservationId { get; set; }
        public Enum.DeeplinkOption DeeplinkOption { get; set; }
        public IEnumerable<string> AdditionalData { get; set; }
    }
}