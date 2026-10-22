using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Query
{
    public class LastNextBookingQuery : BookingQuery, IRequest<BookingResponse>
    {
        public LastNextBookingQuery(string fcNumber, string phoneNumber) : base(fcNumber, phoneNumber)
        {

        }
    }
}
