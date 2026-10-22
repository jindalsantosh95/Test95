using MediatR;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class FlightRulesInfoRequest : BaseSearchRequest, IRequest<FlightInformation>
    {
        public string FareFamily { get; set; }
    }
}
