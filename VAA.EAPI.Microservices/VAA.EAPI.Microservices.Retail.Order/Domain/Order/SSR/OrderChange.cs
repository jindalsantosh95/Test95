using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order.SSR
{
    public class OrderChange 
    {
        public Pos Pos { get; set; }
        public string Rloc { get; set; }
        public string ChannelId { get; set; }
        public IEnumerable<OrderAmendment> OrderAmendments { get; set; }
    }

}
