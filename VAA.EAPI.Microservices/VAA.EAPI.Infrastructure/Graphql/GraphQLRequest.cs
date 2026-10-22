using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.Graphql
{
    public class GraphQLRequest
    {
        public Dictionary<string, string> Headers { get; set; } = new();
        public string GraphQlUri { get; set; }
        public string Token { get; set; }
        public string Request { get; set; }
    }
}
