using System;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public class OperationalScheduleRequest
    {
        public string AirlineCode { get; set; }
        public int FlightNumber { get; set; }
        public DateTime LocalDateFrom { get; set; }
        public DateTime LocalDateTo { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }

        public static explicit operator OperationalScheduleRequest(AircraftMessage aircraftMessage)
        {
            return new()
            {
                AirlineCode = "VS",
                FlightNumber = Convert.ToInt32(aircraftMessage.FlightNumber),
                LocalDateFrom = aircraftMessage.FlightDate,
                LocalDateTo = aircraftMessage.FlightDate,
                OriginAirportCode = aircraftMessage.Origin,
                DestinationAirportCode = aircraftMessage.Destination
            };
        }
    }
}
