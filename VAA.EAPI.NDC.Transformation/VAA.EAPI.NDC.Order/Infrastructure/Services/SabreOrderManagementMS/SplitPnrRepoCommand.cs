using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS
{
    public class SplitPnrRepoCommand : IRequest<OrderDocument>
    {
        public SplitPnrRequest SplitPnrRequest { get; set; }
    }
}

