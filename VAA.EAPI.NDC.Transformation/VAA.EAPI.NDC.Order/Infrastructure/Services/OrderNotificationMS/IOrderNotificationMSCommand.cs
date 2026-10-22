using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS;


namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS
{
    public interface IOrderNotificationMSCommand
    {
        Task<OrderNotificationResponse> Handle(OrderNotificationRequest request);
    }
}
