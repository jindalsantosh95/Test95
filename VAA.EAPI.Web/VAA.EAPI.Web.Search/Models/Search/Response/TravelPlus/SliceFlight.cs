using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class SliceFlight
    {
        public IEnumerable<Segment> Segments { get; set; }
        public string Duration { get; set; }
        public Airport Origin => Segments.FirstOrDefault()?.origin;
        public Airport Destination => Segments.LastOrDefault()?.destination;
        public DateTime? Departure => Segments.FirstOrDefault()?.departure;
        public DateTime? Arrival => Segments.LastOrDefault()?.arrival;
        public IEnumerable<Link>? Links { get; set; }
    }
}
