using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Domain.Models.CosmosGraph
{
    public class TreeEdgeNode
    {
        /// <summary>
        /// The edge.
        /// </summary>
        [JsonProperty("key")]
        public Edge Edge { get; set; }

        /// <summary>
        /// The vertex the edge leads to (can be both the 'in' and 'out' vertex, depending on the traversal).
        /// </summary>
        [JsonProperty("value")]
        public Dictionary<string, TreeVertexNode> VertexNode { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString() => Edge.ToString();

    }
}
