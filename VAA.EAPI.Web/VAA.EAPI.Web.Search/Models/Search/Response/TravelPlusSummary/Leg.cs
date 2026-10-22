using System;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class Leg
    {
        public Origin Origin { get; set; }
        public Destination Destination { get; set; }
        public string Duration { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public string StopOver { get; set; }
        public bool IsDominantLeg { get; set; }
    }
}