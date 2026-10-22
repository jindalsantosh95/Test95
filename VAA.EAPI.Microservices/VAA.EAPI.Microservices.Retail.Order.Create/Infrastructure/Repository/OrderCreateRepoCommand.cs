using MediatR;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Microservices.Retail.Order.Create.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Infrastructure.Repository
{
    public class OrderCreateRepoCommand : IRequest<BookingReference>
    {
        public OrderCreateRequest OrderCreateRequest { get; set; }

        public OrderCreateRepoCommand(OrderCreateRequest orderCreateRequest)
        {
            OrderCreateRequest = orderCreateRequest;
        }
    }
}
