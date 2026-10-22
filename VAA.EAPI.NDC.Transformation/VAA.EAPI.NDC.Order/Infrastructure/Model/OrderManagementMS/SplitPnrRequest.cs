using System.Collections.Generic;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class SplitPnrRequest
    {
        public OrderDocument OrderDocument { get; set; }
        public IEnumerable<string> splitPaxId { get; set; }
    }

}
