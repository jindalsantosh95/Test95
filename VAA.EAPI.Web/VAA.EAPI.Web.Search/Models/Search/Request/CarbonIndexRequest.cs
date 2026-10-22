using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Web.Search.Models.CarbonIndex;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class CarbonIndexRequest : IRequest<IEnumerable<CarbonIndexResponse>>
    {
        public string OfferId { get; set; }
        public string PassengerCounts { get; set; }
    }
}
