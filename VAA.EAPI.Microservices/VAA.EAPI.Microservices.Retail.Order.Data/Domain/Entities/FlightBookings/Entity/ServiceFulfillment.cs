

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class ServiceFulfillment
    {
        public string FulfilmentStatusTypeCode { get; set; }
        public string CustomerOrderItemSequenceNumber { get; set; }
        public string FulfilmentStatusCreationGMTTimestamp { get; set; }
        public string FulfilmentServiceEventCode { get; set; }
        public string FulfilmentStatusReasonCode { get; set; }
        public string FulfilmentStatusReasonText { get; set; }
        public string OperationalFlightNumber { get; set; }
        public string OperationalCarrierCode { get; set; }
        public string ScheduleDepartureDateTime { get; set; }
        public string OriginTripStationCode { get; set; }
        public string DestinationTripStationCode { get; set; }
        public string MarketingProductCode { get; set; }
        public string ProductFulfilmentId { get; set; }
        public string ProductFulfilmentGMTTimestamp { get; set; }
        public string SourceSystem { get; set; }
        public string CabinClassCode { get; set; }
    }
}
