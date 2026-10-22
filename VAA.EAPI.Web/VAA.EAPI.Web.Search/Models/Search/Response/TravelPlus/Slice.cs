using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class Slice
    {
        public string Id { get; set; }
        public string FareId { get; set; }
        public IEnumerable<string> FareFamilyTypes { get; set; }
        public IEnumerable<SliceFlightsAndFares> FlightsAndFares { get; set; }

    }
}
