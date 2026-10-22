using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain
{
    public class OrderChangeNotifRequest
    {
        public OrderDocument OrderDocument { get;set; }
        public IEnumerable<AgencyOCNDetails> AgentEndpointDetails { get; set; }
        public string TTLExpiryType { get; set; }
        public bool IsDuplicatePNR { get; set; }
    }
}
