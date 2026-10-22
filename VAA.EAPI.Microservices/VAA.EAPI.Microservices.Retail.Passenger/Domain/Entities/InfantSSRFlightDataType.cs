namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class InfantSSRFlightDataType
    {
        public string AirlineCode { get; set; }
        public string ArrivalCityCode { get; set; }
        public string ClassOfService { get; set; }
        public string DepartureCityCode { get; set; }
        public string FlightDate { get; set; }
        public string FlightNumber { get; set; }
        public string FlightSegmentNumber { get; set; }
        public string INFTSSRActionIndicator { get; set; }
        public string InfantSSRFreeFromText { get; set; }
        public string InfantSSRPresenceIndicator { get; set; }
        public string InventoryCount { get; set; }
        public string ScheduleArrivalTime { get; set; }
        public string ScheduleArrivalTimeIndicator { get; set; }
        public string ScheduleDepartureTime { get; set; }
        public string ScheduleDepartureTimeIndicator { get; set; }
        public string SegmentStatus { get; set; }
    }
}
