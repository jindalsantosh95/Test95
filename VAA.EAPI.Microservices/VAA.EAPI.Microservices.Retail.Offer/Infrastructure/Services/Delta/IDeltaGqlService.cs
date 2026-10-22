using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Graphql;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta
{
    public interface IDeltaGqlService
    {
        Task<GraphQLResponse> SendAsync(object variables, string gqlQuery, CancellationToken cancellationToken, string? extraLogData = null);
    }
}