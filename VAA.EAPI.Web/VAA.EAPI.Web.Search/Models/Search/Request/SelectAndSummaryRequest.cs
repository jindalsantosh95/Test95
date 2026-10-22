using MediatR;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class SelectAndSummaryRequest : BaseSearchRequest, IRequest<Summary>
    {
        public string SummaryLink { get; set; }
    }
}
