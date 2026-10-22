

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class BookingReference
    {
        public string AirlineDesignator { get; set; }
        public string RLOC { get; set; }
        public string InfantOnlyFlag { get; set; }
        public string RLOCCreateDate { get; set; }
        public string BookingChannel { get; set; }
        public string GroupName { get; set; }
        public string NumberInGroup { get; set; }
        public string ExternalRecordLocator { get; set; }
        public string ActiveLevel { get; set; }
        public string ExternalSystem { get; set; }
        public BookingDetail BookingDetail { get; set; }
    }
}
