using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class SelectAndNextRequest : BaseSearchRequest, IRequest<Result>
    {
        public IEnumerable<SearchSlice> slices { get; set; }
    }
}
