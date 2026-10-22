using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Security.Helper;
using VAA.EAPI.GraphQL.Domain.Security.Interfaces;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Response;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;
using VAA.EAPI.GraphQL.Domain.Security.Validators.NDC;

namespace VAA.EAPI.GraphQL.Domain.Security.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class APIMSubscriptionNDCQuery
    {
        private readonly ILogger<APIMSubscriptionNDCQuery> _logger;
        private readonly IAPIMSubscriptionQueryProvider _apimSubscriptionQueryProvider;

        public APIMSubscriptionNDCQuery(ILogger<APIMSubscriptionNDCQuery> logger, IAPIMSubscriptionQueryProvider apimSubscriptionQueryProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apimSubscriptionQueryProvider = apimSubscriptionQueryProvider ?? throw new ArgumentNullException(nameof(apimSubscriptionQueryProvider));

        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Security.Subscription.Read" })]
        [GraphQLName("securityGetAPIMSubscriptionNDC")]
        [GraphQLDescription("Get the subscription details for the Party/User")]
        public async Task<SubscriptionResponse> GetAPIMSubscriptionNDC(NDCSubscriptionRetrieveQueryRequest request)
        {
            try
            {
                var validationResult = new NDCSubscriptionRetrieveQueryValidator().Validate(request);
                if (!validationResult.IsValid)
                {
                    var errors = APIMSubscriptionHelper.BuildGraphQLRequestValidationErrors(validationResult.Errors);
                    throw new GraphQLRequestException(errors);
                }
                var subscriptionRetrieveRequest = new SubscriptionRetrieveRequest
                {
                    SubscriptionId = APIMSubscriptionHelper.GetSubscriptionId(
                        request.Party.Aggregator?.SingleKeyAggregator == true ? null : request.Party.Agency,
                        request.Party.Aggregator!),
                    Channel = request.Channel,
                    Environment = request.Environment
                };
                var subscriptionResponse = await _apimSubscriptionQueryProvider.GetSubscription(subscriptionRetrieveRequest);

                // If there were any errors, then report them to consumer, 
                if (subscriptionResponse.Error is not null)
                {
                    var error = APIMSubscriptionHelper.BuildGraphQLRequestError("APIM Subscription retrieve request failed.", subscriptionResponse.Error);
                    throw new GraphQLException(error);
                }
                return subscriptionResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Get APIM Susbcription Query");
                throw;
            }
        }


    }
}


