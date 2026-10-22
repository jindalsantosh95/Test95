using System.Collections.Generic;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS
{
    public class OrderNotificationRequest
    {
        public OrderDocument OrderDocument { get; set; }
        public IEnumerable<AgencyOCNDetails> AgentEndpointDetails { get; set; }
        public string TTLExpiryType { get; set; }
        public bool IsDuplicatePNR { get; set; }
    }
}
