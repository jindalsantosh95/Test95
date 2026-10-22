using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Response;

namespace VAA.EAPI.GraphQL.Domain.Security.Interfaces
{
    public interface IAPIMSubscriptionQueryProvider
    {
        Task<SubscriptionResponse> GetSubscription(SubscriptionRetrieveRequest getRequest);
    }
}
