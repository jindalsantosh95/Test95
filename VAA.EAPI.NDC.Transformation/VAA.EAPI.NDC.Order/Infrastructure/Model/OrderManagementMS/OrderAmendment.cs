using System.Collections.Generic;
using VAA.EAPI.NDC.Order.Constants;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class OrderAmendment
    {       
        public IEnumerable<OrderByReference> OrderByReference { get; set; }
        public CustomerPrice CustomerPrices { get; set; }
        public IEnumerable<FlightItems> FlightItems { get; set; }
        public IEnumerable<string> CustomerReference { get; set; }
        public IEnumerable<SpecialServiceRequestInfo> SpecialServiceRequestInfo { get; set; }
        public ProductTypeEnum? ProductType { get; set; }
        public OrderStatusEnum? OrderStatus { get; set; }
    }
}
