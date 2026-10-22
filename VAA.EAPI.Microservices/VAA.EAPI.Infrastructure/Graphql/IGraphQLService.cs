using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Graphql
{
    public interface IGraphQLService
    {
        Task<GraphQLResponse> SendAsync(GraphQLRequest request, CancellationToken cancellationToken);
    }
}