using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Security.Interfaces;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Response;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Security.Services
{
    public class APIMSubscriptionProvider : IAPIMSubscriptionQueryProvider, IAPIMSubscriptionMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public APIMSubscriptionProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }
        public async Task<SubscriptionResponse> GetSubscription(SubscriptionRetrieveRequest getRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.APIMSubscriptionMSGetUrl)
                         .Replace("{channel}", getRequest.Channel)
                         .Replace("{environment}", getRequest.Environment)
                         .Replace("{subscriptionid}", getRequest.SubscriptionId);
            var subscriptionResponse = await _connector.GetAsync<SubscriptionResponse>(uri, GetHeaders(), null);

            return subscriptionResponse.Response;
        }

        public async Task<SubscriptionResponse> CreateSubscription(SubscriptionCreateRequest createRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.APIMSubscriptionMSCreateUrl)
                         .Replace("{channel}", createRequest.Channel)
                         .Replace("{environment}", createRequest.Environment);
            var subscriptionResponse = await _connector.SendAsync<SubscriptionResponse>(uri, GetHeaders(), null, HttpMethod.Post, Json.Serialize(createRequest));

            return subscriptionResponse.Response;
        }
        public async Task<SubscriptionResponse> UpdateSusbscription(SubscriptionUpdateRequest updateRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.APIMSubscriptionMSUpdateUrl)
                         .Replace("{channel}", updateRequest.Channel)
                         .Replace("{environment}", updateRequest.Environment)
                         .Replace("{subscriptionid}", updateRequest.SubscriptionId);
            var subscriptionResponse = await _connector.SendAsync<SubscriptionResponse>(uri, GetHeaders(), null, HttpMethod.Patch, Json.Serialize(updateRequest));

            return subscriptionResponse.Response;
        }
        public async Task<SubscriptionResponse> DeleteSusbscription(SubscriptionDeleteRequest deleteRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.APIMSubscriptionMSDeleteUrl)
                         .Replace("{channel}", deleteRequest.Channel)
                         .Replace("{environment}", deleteRequest.Environment)
                         .Replace("{subscriptionid}", deleteRequest.SubscriptionId);
            var subscriptionResponse = await _connector.SendAsync<SubscriptionResponse>(uri, GetHeaders(), null, HttpMethod.Delete, Json.Serialize(deleteRequest));

            return subscriptionResponse.Response;
        }
        private Dictionary<string, string> GetHeaders() => new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.APIMSubscriptionMSOcpKey) } };

    }
}
