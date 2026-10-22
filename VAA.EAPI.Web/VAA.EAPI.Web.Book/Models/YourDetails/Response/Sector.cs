using System;


namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class Sector
    {
        public string SectorId { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public string AirlineName { get; set; }
        public string OperatingAirline { get; set; }
        public string OperatingAirlineName { get; set; }
        public SectorLeg Leg { get; set; }
        public Airport Departing { get; set; }
        public Airport Arriving { get; set; }
        public DateTime DepartingAt { get; set; }
        public DateTime ArrivingAt { get; set; }
        public string CabinClass { get; set; }
        public object Pnr { get; set; }
        public string AirCraftDetail { get; set; }
        public string Duration { get; set; }
    }
}
