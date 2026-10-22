using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain
{
    public class AgentResponse
    {
        public string AgentId { get; set; }
        public Status Status { get; set; }
        public bool AcknowledgementRcvd { get; set; }
    }
}
