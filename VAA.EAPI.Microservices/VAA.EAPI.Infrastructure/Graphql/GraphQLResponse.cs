using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Infrastructure.Graphql
{
    public class GraphQLResponse
    {
        public JObject Data { get; set; }
        public IEnumerable<GraphQLError> Errors { get; set; } = Enumerable.Empty<GraphQLError>();
    }

}
