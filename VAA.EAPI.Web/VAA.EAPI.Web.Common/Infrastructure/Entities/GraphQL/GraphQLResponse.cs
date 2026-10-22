using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL
{
    public class GraphQLResponse
    {
        public JObject Data { get; set; }
        public IEnumerable<GraphQLError> Errors { get; set; } = Enumerable.Empty<GraphQLError>();
    }

    public class GraphQLError
    {
        public string Message { get; set; }
        public GraphQLErrorExtension Extensions { get; set; }

    }
    public class GraphQLErrorExtension
    {
        public string Code { get; set; }
        public string Reason { get; set; }
        public string Response { get; set; }
    }
}
