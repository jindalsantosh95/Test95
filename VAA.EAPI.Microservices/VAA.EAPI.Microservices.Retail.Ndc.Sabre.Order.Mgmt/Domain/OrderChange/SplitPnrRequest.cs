using System.Collections.Generic;
namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class SplitPnrRequest
    {  
        public OrderDocument OrderDocument { get; set; }
        public IEnumerable<string> splitPaxId { get; set; }
    }
}
