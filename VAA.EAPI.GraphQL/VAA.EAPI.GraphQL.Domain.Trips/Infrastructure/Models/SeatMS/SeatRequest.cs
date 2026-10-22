using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSRequest")]
    public class SeatRequest
    {
        public Context Context { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public string ChannelId { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }

        public static explicit operator SeatRequest((Booking bookingDetails, string segmentId) source)
        {
            var (bookingDetails, segmentId) = source;

            var slice = bookingDetails.Slices?.FirstOrDefault(slice => slice.Segments.Any(seg => seg.Id == segmentId));
            var requestedSegment = slice?.Segments.FirstOrDefault(seg => seg.Id == segmentId);

            return new()
            {
                ChannelId = Constants.Application.SeatMSDLChannelId,
                Context = new() { CurrencyCode = bookingDetails.Pos?.CurrencyCode, RecordLocator = bookingDetails.PnrLocator },
                CustomerDetails = bookingDetails.Passenger?.Select((p, index) => (Customer)(p, index.ToString())),
                FlightSegments = [(Segment)requestedSegment]
            };
        }
    }
}