using MediatR;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class SortRequest : IRequest<SearchResultsResponse>
    {
        public string BasketId { get; set; }
        public string SelectedSort { get; set; }
        public FilterRequest Filters { get; set; }
        public string SliceId { get; set; }
        public string OfferId { get; set; }
        public string AuthToken { get; set; }
    }
}
