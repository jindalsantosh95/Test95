using System;

namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class BrandQueryRequestSegment
    {
        public string Carrier { get; set; }
        public int FlightNumber { get; set; }
        public DateTime DepartureDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public int Cabin { get; set; }
        public string FareBasis { get; set; }
    }
}