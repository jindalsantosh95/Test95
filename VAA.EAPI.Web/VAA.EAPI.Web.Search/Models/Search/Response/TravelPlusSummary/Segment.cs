using System;
using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class Segment
    {
        public IEnumerable<Metal> Metal { get; set; }
        public Airline Airline { get; set; }
        public string FlightNumber { get; set; }
        public string OperatingFlightNumber { get; set; }
        public Airline OperatingAirline { get; set; }
        public Origin Origin { get; set; }
        public Destination Destination { get; set; }
        public string Duration { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public int StopCount { get; set; }
        public string Connection { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
        public string BookingClass { get; set; }
        public string FareBasisCode { get; set; }
        public string DominantFareProduct { get; set; }
        public bool IsSaverFare { get; set; }
        public bool? PendingGovtApproval { get; set; }
    }
}