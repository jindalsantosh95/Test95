using System.Collections.Generic;
using System.Linq;
/**
* The filter class, which gets populated in the result object during the flight search operation.
* The Front end makes use of this class to populate the filters, and fire away the
* network calls using the links to apply/remove the filters.
*
* @author Nitish Singla
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class Filter
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public int Count { get { return Values.Count(); } }
        public IEnumerable<FilterValue> Values { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<Link> Links { get; set; }
        /*
        {
            get
            {
                return new List<Link>()
                {
                    new Link() {Rel = Name, Href = $"{GetPostbackUrl()}/{GetBasketId()}/filter/{Name}"}
                };
            }
        }
        */
    }
}
