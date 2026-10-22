using System.Collections.Generic;

/**
* This class is used to deserialize the response from GraphQL
**/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class SearchResults
    {
        public PriceGrid PriceGrid { get; set; }
        public CalendarGraph Calendar { get; set; }
        public Result Result { get; set; }
        public IEnumerable<Link> Links { get; set; }
    }
}
