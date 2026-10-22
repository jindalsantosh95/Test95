using System;
using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class Flight
    {
        public string FlightId { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public string Duration { get; set; }
        public string FareFamilyType { get; set; }
        public FareFamilyFees? FareFamilyFees { get; set; }
        public string CabinName { get; set; }
        public Origin Origin { get; set; }
        public DateTime Arrival { get; set; }
        public Destination Destination { get; set; }
        public DateTime Departure { get; set; }
        public string Id { get; set; }
        public string FareId { get; set; }
    }
}