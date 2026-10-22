using MediatR;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class BookingSessionRequest : BaseSearchRequest, IRequest<Booking>
    {
    }
}
