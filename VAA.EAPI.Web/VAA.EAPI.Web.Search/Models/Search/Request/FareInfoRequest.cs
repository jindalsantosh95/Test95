using MediatR;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class FareInfoRequest : BaseRequest, IRequest<FlightInformation>
    {
        public string OfferId { get; internal set; }
    }
}
