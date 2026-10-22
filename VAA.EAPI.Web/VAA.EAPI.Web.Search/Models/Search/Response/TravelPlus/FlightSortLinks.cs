using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class FlightSortLinks
    {
        public string SelectedSort { get; set; }

        public IEnumerable<string> AvailableSortFields { get; set; }

        public IEnumerable<SortLink> Links { get; set; }
    }
}
