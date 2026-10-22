using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class DisruptionResponse
    {
        public Event Event { get; set; }
        public ResponseDetails InRuleResponse { get; set; }
        public IEnumerable<OrderDetails> OrderDetails { get; set; }
    }
}
