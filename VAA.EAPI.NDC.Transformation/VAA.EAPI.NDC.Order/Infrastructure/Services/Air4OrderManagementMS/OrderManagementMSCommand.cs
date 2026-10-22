using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderManagementMS
{
    public class OrderManagementMSCommand : IRequest<ChangeOrderRS>
    {
        public OrderChangeRequest OrderChangeRequest { get; set; }
    }
}
