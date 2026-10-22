namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class FlightInfoType
    {
        public string AirlineCode { get; set; }
        public string ArrivalPoint { get; set; }
        public string ArrivalTime { get; set; }
        public string ClassOfService { get; set; }
        public string ClassOfService2 { get; set; }
        public string ConnectionPoint { get; set; }
        public string ConnectionPoint2 { get; set; }
        public string Date { get; set; }
        public string DeparturePoint { get; set; }
        public string DepartureTime { get; set; }
        public string FlightNumber { get; set; }
        public string ItineraryStatus { get; set; }
        public string NumberInParty { get; set; }
        public string PreviousItineraryStatus { get; set; }
        public string SeatId { get; set; }
        public string SeatStatus { get; set; }
        public string SeatType { get; set; }
        public string SegmentNumber { get; set; }
        public string StandbyIndicator { get; set; }
    }
}
