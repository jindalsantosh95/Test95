using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings;
namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightBookings
{
    public class FlightBookingsQuery : IRequest<FlightBookingsResponse>
    {
        public string FCNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }

    }
}
