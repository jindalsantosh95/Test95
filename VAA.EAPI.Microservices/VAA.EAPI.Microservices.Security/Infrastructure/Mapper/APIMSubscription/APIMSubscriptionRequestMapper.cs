using System;
using VAA.EAPI.Microservices.Security.Constants;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionRequest;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Mapper.APIMSubscription
{
    public static class APIMSubscriptionRequestMapper
    {
        public static APIMSubscriptionCreateRequest MapToMSAPIMSubscriptionCreateRequest(APIMSubscriptionConfig apimConfig, string apimUserID, string subscriptionName)
        {
            var apimRequest = new APIMSubscriptionCreateRequest();
            apimRequest.Properties = new SubscriptionCreateProperties
            {
                OwnerId = apimUserID,
                Scope = string.Format(Constants.Application.SubscriptionScope, apimConfig.APIMSubscriptionId, apimConfig.APIMResourceGroupName, apimConfig.APIMName, apimConfig.APIMProductId),
                DisplayName = subscriptionName
            };

            return apimRequest;
        }

        public static APIMSubscriptionUpdateRequest MapToMSAPIMSubscriptionUpdateRequest(APIMSubscriptionConfig apimConfig, string enabled)
        {
            var apimRequest = new APIMSubscriptionUpdateRequest();
            apimRequest.Properties = new SubscriptionUpdateProperties
            {
                State = string.Equals(enabled, "true", StringComparison.CurrentCultureIgnoreCase) ? "active" : "suspended"
            };

            return apimRequest;
        }


    }
}
