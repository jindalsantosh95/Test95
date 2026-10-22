

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class FlightProduct
    {
        public string FlightServiceProduct { get; set; }
        public string CustomerOrderItemSequenceNumber { get; set; }
        public string BookingSegmentNumber { get; set; }
        public string BookingCode { get; set; }
        public string FlightDetailSequenceNumber { get; set; }
        public string MarketedCarrierCode { get; set; }
        public string MarketedFlightNumber { get; set; }
        public string ScheduledDepartureLocalDate { get; set; }
        public string OriginTransportationStationCode { get; set; }
        public string DestinationTransportationStationCode { get; set; }
        public string ClassOfServiceCode { get; set; }
        public string SeatIdentifier { get; set; }
        public string ApplicationChannel { get; set; }
        public string SOAChannelSessionName { get; set; }
        public string CreationUserIdentifier { get; set; }
        public string FlightDetailCreationGMTTimestamp { get; set; }
        public string FlightSegmentNumber { get; set; }
        public string CabinClassCode { get; set; }
    }
}
