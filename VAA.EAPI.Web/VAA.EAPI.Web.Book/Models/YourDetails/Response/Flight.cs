using System;
using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class Flight
    {
        public string AirlineName { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public Airport Departing { get; set; }
        public Airport Arriving { get; set; }
        public DateTime DepartingAt { get; set; }
        public DateTime ArrivingAt { get; set; }
        public string CabinClass { get; set; }
        public IEnumerable<Sector> Sectors { get; set; }
    }
}
