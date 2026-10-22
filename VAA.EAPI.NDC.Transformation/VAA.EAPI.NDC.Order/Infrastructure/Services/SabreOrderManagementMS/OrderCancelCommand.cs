using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS
{
    public class OrderCancelCommand : IRequest<bool>
    {
        public OrderChangeRequest OrderChangeRequest { get; set; }
    }
}

