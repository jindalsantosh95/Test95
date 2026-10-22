using MediatR;
using VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class OrderSeatV8AddSeatQuery : OrderChangeRequestData, IRequest<AddResponseType>
    {
    }
}
