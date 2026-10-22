using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS
{
    public class OrderNotificationResponse
    {
        public OrderNotificationAgentResponse OrderNotificationAgentResponse { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }

    }
}
