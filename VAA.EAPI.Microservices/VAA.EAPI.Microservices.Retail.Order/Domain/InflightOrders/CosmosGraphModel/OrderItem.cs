using VAA.EAPI.Domain.Models.CosmosGraph;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CosmosGraphModel
{
    public class OrderItem : GraphObject
    {
        public string OrderId { get; set; }
        public int OrderItemId { get; set; }
        public string Version { get; set; }
        public string ItemType { get; set; }
        public string Status { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
        public string ExternalOrderId { get; set; }
        public string ExternalOrderItemId { get; set; }
        public string DocType { get; set; }


        public override PropertyValue GetProperties()
        {
            PropertyValue propertyValue = new PropertyValue();
            propertyValue.Properties.Add("version", Version);
            propertyValue.Properties.Add("itemType", ItemType);
            propertyValue.Properties.Add("status", Status);
            propertyValue.Properties.Add("ownerAirline", OwnerAirline);
            propertyValue.Properties.Add("ownerNonAirline", OwnerNonAirline);
            propertyValue.Properties.Add("externalOrderId", ExternalOrderId);
            propertyValue.Properties.Add("externalOrderItemId", ExternalOrderItemId);
            propertyValue.Properties.Add("docType", DocType);
            propertyValue.Properties.Add("documentType", "OrderItem");
            propertyValue.Properties.Add("documentVersion", "1.0");
            return propertyValue;
        }
    }
}
