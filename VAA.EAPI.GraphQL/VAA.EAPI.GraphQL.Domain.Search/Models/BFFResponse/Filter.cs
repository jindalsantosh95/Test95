/**
* The filter class, which gets populated in the result object during the flight search operation.
* The Front end makes use of this class to populate the filters, and fire away the
* network calls using the links to apply/remove the filters.
*
* @author Nitish Singla
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class Filter
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public IEnumerable<FilterValue> Values { get; set; }
        public int Count => Values.Count();
        public string DisplayName => Constants.Application.Filters[Name];
        public IEnumerable<Link> Links
        {
            get
            {
                if (Values.Any())
                {
                    return Values.First().ClearFilterLink;
                }
                return Enumerable.Empty<Link>();
            }
        }
    }
}
