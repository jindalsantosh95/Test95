using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using TripsMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class MessageResponse
    {
        public BookingReference? BookingReference { get; set; }
        public IEnumerable<Trip>? Trips { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }
        public IEnumerable<MessageDetail>? Messages { get; set; }

        public static explicit operator MessageResponse(TripsMS.MessageResponse source)
        {
            if (source == null) return null;

            return new MessageResponse
            {
                BookingReference = new BookingReference { Id = source.BookingReference.Id, Organization = new Organization { Id = source.BookingReference.Organization.Id } },
                Trips = source.Trips?.Select(t => (Trip)t),
                Passengers = source.Passengers?.Select(p => (Passenger)(p, Enumerable.Empty<CabinRefData>())),
                Messages = source.Messages?.Select(m => (MessageDetail)m)
            };
        }

    }
}
