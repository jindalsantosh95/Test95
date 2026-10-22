using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Query
{
    public class FutureBookingQuery : BookingQuery, IRequest<BookingResponse>
    {
        public FutureBookingQuery(string fcNumber, string phoneNumber, string page, string pageSize) : base(fcNumber, phoneNumber, page, pageSize)
        {

        }
    }
}
