using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Domain.Models.CosmosGraph
{
    public class Vertex : GraphObject
    {
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        [JsonProperty(PropertyNames.Properties)]
        public virtual IDictionary<string, object> Properties
        {
            get { return _properties; }
            set { _properties = value ?? new Dictionary<string, object>(); }
        }

        private IDictionary<string, object> _properties = new Dictionary<string, object>();

        public override PropertyValue GetProperties()
        {
            PropertyValue propertyValue = new PropertyValue();
            foreach (var property in Properties)
            {
                propertyValue.Properties.Add(property.Key, property.Value);
            }
            return propertyValue;

        }
    }
}
