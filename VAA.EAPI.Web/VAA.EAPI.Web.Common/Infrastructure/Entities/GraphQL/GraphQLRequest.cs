using MediatR;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL
{
    public class GraphQLRequest : IRequest<GraphQLResponse>
    {
        public string PqId { get; set; }
        public string Query { get; set; }
        public JObject Variables { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();
        public string GraphqlUrl { get; set; }
    }

}
