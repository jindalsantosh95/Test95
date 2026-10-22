using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS
{
    public class PublishOrderDocMSCommand : IRequest<Unit>
    {
        public UpdateOrderRequest UpdateOrderRequest { get; set; }

        public PublishOrderDocMSCommand(UpdateOrderRequest updateOrderRequest)
        {
            UpdateOrderRequest = updateOrderRequest;
        }
    }
}
