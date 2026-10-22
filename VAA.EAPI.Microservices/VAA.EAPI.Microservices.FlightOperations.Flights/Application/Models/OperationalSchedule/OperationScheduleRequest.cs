using NodaTime;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.OperationalSchedule
{
    public class OperationalScheduleRequest
    {
        public string AirlineCode { get; set; }
        public int FlightNumber { get; set; }
        public DateTime LocalDateFrom { get; set; }
        public DateTime LocalDateTo { get; set; }
        public string Direction { get; set; }
        public int Limit { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }        
    }

}
  
