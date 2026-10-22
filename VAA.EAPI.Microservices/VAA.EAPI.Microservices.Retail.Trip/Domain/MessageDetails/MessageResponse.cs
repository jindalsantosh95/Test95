using DLSchema = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre;
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails

{
    public class MessageResponse
    {

        public BookingReference BookingReference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
        public IEnumerable<MessageDetail> Messages { get; set; }

        public static explicit operator MessageResponse(DLSchema.MessageResponse source)
        {
            if (source == null)
                return null;

            return new MessageResponse
            {
                Messages = source.Messages?.Select(m => (MessageDetail)m),
                BookingReference = (BookingReference)source.BookingReference,
                Passengers = source.Passengers?.Select(p => (Passenger)p),
                Trips = source.Trips?.Select(t => (Trip)t)
            };
        }
    }
}
