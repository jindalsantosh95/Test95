using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain
{
    public class OrderChangeNotifResponse
    {
        public IEnumerable<AgentResponse> AgentResponse { get; set; }
    }
}
