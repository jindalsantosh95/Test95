using VAA.EAPI.Domain.Models.CosmosGraph;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CosmosGraphModel
{
    public class Customer : GraphObject
    {
        public string CustomerId { private get; set; }

        public override PropertyValue GetProperties()
        {
            PropertyValue propertyValue = new PropertyValue();
            propertyValue.Properties.Add("CustomerID", CustomerId);
            propertyValue.Properties.Add("documentType", "Customer");
            propertyValue.Properties.Add("documentVersion", "1.0"); //Change this value whenever adding new property
            return propertyValue;
        }

    }
}
