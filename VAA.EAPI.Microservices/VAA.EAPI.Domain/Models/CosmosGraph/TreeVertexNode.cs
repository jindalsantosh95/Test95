using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Domain.Models.CosmosGraph
{
    public class TreeVertexNode
    {
        /// <summary>
        /// The edges. Can be a mixture of outbound and inbound edges depending on the traversal.
        /// </summary>
        [JsonProperty("value")]
        public Dictionary<string, TreeEdgeNode> EdgeNodes { get; set; }

        /// <summary>
        /// The vertex.
        /// </summary>
        [JsonProperty("key")]
        public Vertex Vertex { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString() => Vertex.ToString() + " Edges: " + EdgeNodes.Count;

    }
}
