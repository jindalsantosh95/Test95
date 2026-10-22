using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class Itinerary
    {
        public string OriginCode { get; set; }
        public string DestinationCode { get; set; }
        public string Carrier { get; set; }
        public string SoldPassengerCabin { get; set; }
        public string ReservationBooking { get; set; }
        public string FlightDate { get; set; }
        public string FlightTime { get; set; }
        public string FareBasisTicketDesignator { get; set; }
        public string BaggageAllowance { get; set; }
        public string SegmentIdentifier { get; set; }
        public string StopoverCode { get; set; }
        public string CouponUseIndicator { get; set; }
        public string FlightArrivalTime { get; set; }
        public string FlightArrivalDate { get; set; }
        public string FlightNumber { get; set; }

        public static explicit operator Itinerary(Segment seg)
        {
            return new Itinerary
            {
                OriginCode = seg.OriginAirportCode,
                DestinationCode = seg.DestinationAirportCode,
                ReservationBooking = seg.ClassOfService,
                FareBasisTicketDesignator = seg.FareBasis,
                Carrier = seg.MarketingCarrier,
                StopoverCode = seg?.ConnectionOrStopOver,
                SegmentIdentifier = seg.SegmentId.Replace("SEG", ""),
                FlightNumber = seg.MarketingFlightNumber ?? seg.OperatingFlightNumber
            };
        }

    }
}
