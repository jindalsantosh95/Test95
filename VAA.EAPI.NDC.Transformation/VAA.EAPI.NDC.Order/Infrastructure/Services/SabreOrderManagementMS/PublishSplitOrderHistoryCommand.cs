using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS
{
    public class PublishSplitOrderHistoryCommand : IRequest<Unit>
    {
        public UpdateOrderRequest UpdateOrderRequest { get; set; }

        public PublishSplitOrderHistoryCommand(UpdateOrderRequest updateOrderRequest)
        {
            UpdateOrderRequest = updateOrderRequest;
        }
    }
}
