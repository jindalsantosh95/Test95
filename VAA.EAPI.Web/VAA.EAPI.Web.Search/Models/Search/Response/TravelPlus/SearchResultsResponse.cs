using System.Collections.Generic;

/**
* This class is used to send the respose to the front end
**/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class SearchResultsResponse
    {
        public PriceGrid PriceGrid { get; set; }
        public CalendarFE Calendar { get; set; }
        public Result Result { get; set; }
        public IEnumerable<Link> Links { get; set; }

        public static explicit operator SearchResultsResponse(SearchResults source)
        {
            return new()
            {
                PriceGrid = source.PriceGrid,
                Calendar = (CalendarFE)source.Calendar,
                Result = source.Result,
                Links = source.Links,
            };
        }
    }
}
