using System.Collections.Generic;

namespace VAA.EAPI.Domain.Models.CosmosGraph
{
    /// <summary>
    /// Data container for the value of a vertex's property
    /// </summary>
    public sealed class PropertyValue
    {
        /// <summary>
        /// Gets or sets the property value identifier.
        /// </summary>        
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the properties (if any).
        /// </summary>       
        public IDictionary<string, object> Properties
        {
            get { return _properties; }
            set { _properties = value ?? new Dictionary<string, object>(); }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>      
        public object Value { get; set; }

        private IDictionary<string, object> _properties = new Dictionary<string, object>();


    }
}
