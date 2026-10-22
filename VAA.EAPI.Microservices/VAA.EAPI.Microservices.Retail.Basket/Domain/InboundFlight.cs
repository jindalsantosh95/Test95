using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class InboundFlight
    {
        public string AirlineName { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public Departing Departing { get; set; }
        public Arriving Arriving { get; set; }
        public DateTime DepartingAt { get; set; }
        public DateTime ArrivingAt { get; set; }
        public string CabinClass { get; set; }
        public IEnumerable<Sector> Sectors { get; set; }
    }
}