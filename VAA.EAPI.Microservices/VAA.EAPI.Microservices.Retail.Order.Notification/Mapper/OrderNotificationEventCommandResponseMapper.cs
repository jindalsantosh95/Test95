using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Mapper
{
    public class OrderNotificationEventCommandResponseMapper
    {
        public static Response<string> OrderNotificationResponseMapper(string correlationid, string status)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = null
            };
        }
    }
}
