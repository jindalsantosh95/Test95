using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Domain.Models.CosmosGraph
{
    /// <summary>
    /// Represents an schema-less edge on a graph
    /// </summary>
    public class Edge : GraphObject
    {
        /// <summary>
        /// Gets or sets the id of the "in" vertex.
        /// </summary>
        [JsonProperty(PropertyNames.InVertexId)]
        public virtual string InV { get; set; }

        /// <summary>
        /// Gets or sets the "in" vertex label.
        /// </summary>
        [JsonProperty(PropertyNames.InVertexLabel)]
        public virtual string InVLabel { get; set; }

        /// <summary>
        /// Gets or sets the id of the "out" vertex.
        /// </summary>
        [JsonProperty(PropertyNames.OutVertexId)]
        public virtual string OutV { get; set; }

        /// <summary>
        /// Gets or sets the "out" vertex label.
        /// </summary>
        [JsonProperty(PropertyNames.OutVertexLabel)]
        public virtual string OutVLabel { get; set; }

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

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{OutVLabel}-{Label}->{InVLabel}";
        }
    }
}
