using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class ChangeOrderRS
    {
        public OrderDocument orderDocument { get; set; }
        public OrderChangeResponse orderChangeResponse { get; set; }
        public IEnumerable<ErrorDetails> error { get; set; }
    }
}
