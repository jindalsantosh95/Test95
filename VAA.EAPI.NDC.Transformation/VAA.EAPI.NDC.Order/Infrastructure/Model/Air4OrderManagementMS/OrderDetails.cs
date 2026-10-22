using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS
{
    public class OrderDetails
    {
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public IEnumerable<AccountableDocumentReference> AccountableDocumentRefs { get; set; }
    }
}
