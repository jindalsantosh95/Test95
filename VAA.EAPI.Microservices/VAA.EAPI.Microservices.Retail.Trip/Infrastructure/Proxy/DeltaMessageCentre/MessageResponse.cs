namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    public class MessageResponse
    {

        public BookingReference BookingReference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
        public IEnumerable<MessageDetail> Messages { get; set; }

    }
}
