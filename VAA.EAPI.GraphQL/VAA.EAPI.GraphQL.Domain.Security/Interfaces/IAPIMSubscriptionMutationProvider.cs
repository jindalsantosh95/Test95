using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Response;

namespace VAA.EAPI.GraphQL.Domain.Security.Interfaces
{
    public interface IAPIMSubscriptionMutationProvider
    {
        Task<SubscriptionResponse> CreateSubscription(SubscriptionCreateRequest createRequest);
        Task<SubscriptionResponse> UpdateSusbscription(SubscriptionUpdateRequest updateRequest);
        Task<SubscriptionResponse> DeleteSusbscription(SubscriptionDeleteRequest deleteRequest);
    }
}
