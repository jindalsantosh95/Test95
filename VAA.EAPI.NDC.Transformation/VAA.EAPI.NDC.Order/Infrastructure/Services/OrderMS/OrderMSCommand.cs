using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderMS
{
    public class OrderMSCommand : OrderCreateRequest, IRequest<OrderRS>
    {
    }
}

