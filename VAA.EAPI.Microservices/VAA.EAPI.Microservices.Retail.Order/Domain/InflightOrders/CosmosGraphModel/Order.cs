using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Models.CosmosGraph;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CosmosGraphModel
{
    public class Order : GraphObject
    {
        public string OrderId { get; set; }

        public string OrderCreationLocalDateTime { get; set; }

        public string OrderStatus { get; set; }

        public override PropertyValue GetProperties()
        {
            PropertyValue propertyValue = new PropertyValue();
            propertyValue.Properties.Add("orderID", OrderId);
            propertyValue.Properties.Add("orderStatus", OrderStatus);
            propertyValue.Properties.Add("localDateTime", DateTimeHelper.ConvertDateTimeToUnix(OrderCreationLocalDateTime));
            propertyValue.Properties.Add("documentType", "Order");
            propertyValue.Properties.Add("documentVersion", "1.0");
            return propertyValue;
        }

    }
}
