using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR
{
    public class FQTVSSRRequestData
    {
        public Pos Pos { get; set; }
        public string Rloc { get; set; }
        public string ChannelId { get; set; }
        public IEnumerable<OrderAmendment> OrderAmendments { get; set; }
    }
}
