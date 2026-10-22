using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS
{
    public class AgentResponse
    {
        public string AgentId { get; set; }
        public Status Status { get; set; }
        public bool AcknowledgementRcvd { get; set; }
    }
}
