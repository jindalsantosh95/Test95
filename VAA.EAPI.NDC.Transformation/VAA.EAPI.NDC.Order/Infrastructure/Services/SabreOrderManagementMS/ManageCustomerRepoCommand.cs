using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS
{
    public class ManageCustomerRepoCommand : IRequest<ChangeOrderRS>
    {
        public ManageCustomerRequest ManageCustomerRequest { get; set; }
    }
}
