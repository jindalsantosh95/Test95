using System.Runtime.Serialization;
using VAA.EAPI.GraphQL.Domain.Trips.Constants;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    public class FlightItems
    {
        public string ProductCode { get; set; }  
        public string ActionContext { get; set; }
        public string CurrentSeatAssignment { get; set; }
        public FlightItemSegment Segments { get; set; }

        public static explicit operator FlightItems((Basket basket, BasketMS.SeatAssignment seatDeletionRequest) source)
        {
            var (basket, seatDeletionRequest) = source;
            var segment = basket.BookingDetails.Slices
                .SelectMany(slice => slice.Segments)
                .FirstOrDefault(seg => seg.Id == seatDeletionRequest.SegmentId);
            return new FlightItems
            {
                ActionContext = ActionContexts.Delete.ToString(),
                CurrentSeatAssignment = seatDeletionRequest.SeatNumber,
                Segments = segment != null ? (FlightItemSegment)segment : null,
            };
        }
    }
}