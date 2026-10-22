using System.Collections.Generic;

namespace VAA.EAPI.Domain.Models.CosmosGraph
{
    public class GraphEdge : GraphObject
    {

        public GraphObject FromV { get; set; }
        public GraphObject ToV { get; set; }

        public List<PropertyValue> EdgeProperties { private get; set; }

        public override PropertyValue GetProperties()
        {
            if (EdgeProperties == null) return null;
            PropertyValue propertyValue = new PropertyValue();
            foreach (var property in EdgeProperties)
            {
                propertyValue.Properties.Add(property.Id, property.Value);
            }
            return propertyValue;
        }
    }
}
