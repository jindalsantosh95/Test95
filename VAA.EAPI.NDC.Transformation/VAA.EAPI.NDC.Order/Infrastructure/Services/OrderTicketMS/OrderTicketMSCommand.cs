using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderTicketMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderTicketMS
{
    public class OrderTicketMSCommand : OrderTicketRequest, IRequest<OrderTicketRS>
    {
    }
}

