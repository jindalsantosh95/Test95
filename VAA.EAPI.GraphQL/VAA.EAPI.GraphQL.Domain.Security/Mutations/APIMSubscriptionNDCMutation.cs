using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Security.Helper;
using VAA.EAPI.GraphQL.Domain.Security.Interfaces;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Response;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;
using VAA.EAPI.GraphQL.Domain.Security.Validators.NDC;

namespace VAA.EAPI.GraphQL.Domain.Security.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class APIMSubscriptionNDCMutation
    {
        private readonly ILogger<APIMSubscriptionNDCMutation> _logger;
        private readonly IAPIMSubscriptionMutationProvider _apimSubscriptionMutationProvider;

        public APIMSubscriptionNDCMutation(ILogger<APIMSubscriptionNDCMutation> logger, IAPIMSubscriptionMutationProvider apimSubscriptionMutationProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apimSubscriptionMutationProvider = apimSubscriptionMutationProvider ?? throw new ArgumentNullException(nameof(apimSubscriptionMutationProvider));

        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Security.Subscription.Create" })]
        [GraphQLName("securityCreateAPIMSubscriptionNDC")]
        [GraphQLDescription("Create and Returns the subscription created status for the Party/User")]
        public async Task<SubscriptionResponse> CreateAPIMSubscriptionNDC(NDCSubscriptionCreateMutationRequest request)
        {
            try
            {
                var validationResult = new NDCSubscriptionCreateRequestValidator().Validate(request);
                if (!validationResult.IsValid)
                {
                    var errors = APIMSubscriptionHelper.BuildGraphQLRequestValidationErrors(validationResult.Errors);
                    throw new GraphQLRequestException(errors);
                }
                var subscriptionCreateRequest = new SubscriptionCreateRequest
                {
                    SubscriptionId = APIMSubscriptionHelper.GetSubscriptionId(
                        request.Party.Aggregator?.SingleKeyAggregator == true ? null : request.Party.Agency,
                        request.Party.Aggregator!),
                    SubscriptionName = APIMSubscriptionHelper.GetSubscriptionName(
                        request.Party.Aggregator?.SingleKeyAggregator == true ? null : request.Party.Agency,
                        request.Party.Aggregator!),
                    UserAccount = new UserAccountDetails
                    {
                        FirstName = request.UserAccount.FirstName,
                        LastName = request.UserAccount.LastName,
                        EmailAddress = request.UserAccount.EmailAddress,
                    },
                    Channel = request.Channel,
                    Environment = request.Environment
                };
                var subscriptionResponse = await _apimSubscriptionMutationProvider.CreateSubscription(subscriptionCreateRequest);

                // If there were any errors, then report them to consumer, 
                if (subscriptionResponse.Error is not null)
                {
                    var error = APIMSubscriptionHelper.BuildGraphQLRequestError("APIM Subscription create request failed.", subscriptionResponse.Error);
                    throw new GraphQLException(error);
                }
                return subscriptionResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in Create APIM Susbcription Mutation - {ex.Message}", ex);
                throw;
            }
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Security.Subscription.Update" })]
        [GraphQLName("securityUpdateAPIMSubscriptionNDC")]
        [GraphQLDescription("Activate/Suspend Subscription and Returns the subscription status for the Party/User")]
        public async Task<SubscriptionResponse> UpdateAPIMSubscriptionNDC(NDCSubscriptionUpdateMutationRequest request)
        {
            try
            {
                var validationResult = new NDCSubscriptionUpdateRequestValidator().Validate(request);
                if (!validationResult.IsValid)
                {
                    var errors = APIMSubscriptionHelper.BuildGraphQLRequestValidationErrors(validationResult.Errors);
                    throw new GraphQLRequestException(errors);
                }
                var subscriptionUpdateRequest = new SubscriptionUpdateRequest
                {
                    SubscriptionId = APIMSubscriptionHelper.GetSubscriptionId(
                        request.Party.Aggregator?.SingleKeyAggregator == true ? null : request.Party.Agency,
                        request.Party.Aggregator!),
                    Enabled = request.Enabled,
                    Channel = request.Channel,
                    Environment = request.Environment
                };
                var subscriptionResponse = await _apimSubscriptionMutationProvider.UpdateSusbscription(subscriptionUpdateRequest);

                // If there were any errors, then report them to consumer, 
                if (subscriptionResponse.Error is not null)
                {
                    var error = APIMSubscriptionHelper.BuildGraphQLRequestError("APIM Subscription update request failed.", subscriptionResponse.Error);
                    throw new GraphQLException(error);
                }
                return subscriptionResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in Update APIM Susbcription Mutation - {ex.Message}", ex);
                throw;
            }
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Security.Subscription.Delete" })]
        [GraphQLName("securityDeleteAPIMSubscriptionNDC")]
        [GraphQLDescription("Delete the subscription for the Party/User")]
        public async Task<SubscriptionResponse> DeleteAPIMSubscriptionNDC(NDCSubscriptionDeleteMutationRequest request)
        {
            try
            {
                var validationResult = new NDCSubscriptionDeleteRequestValidator().Validate(request);
                if (!validationResult.IsValid)
                {
                    var errors = APIMSubscriptionHelper.BuildGraphQLRequestValidationErrors(validationResult.Errors);
                    throw new GraphQLRequestException(errors);
                }
                var subscriptionDeleteRequest = new SubscriptionDeleteRequest
                {
                    SubscriptionId = APIMSubscriptionHelper.GetSubscriptionId(
                        request.Party.Aggregator?.SingleKeyAggregator == true ? null : request.Party.Agency,
                        request.Party.Aggregator!),
                    Channel = request.Channel,
                    Environment = request.Environment
                };
                var subscriptionResponse = await _apimSubscriptionMutationProvider.DeleteSusbscription(subscriptionDeleteRequest);

                // If there were any errors, then report them to consumer, 
                if (subscriptionResponse.Error is not null)
                {
                    var error = APIMSubscriptionHelper.BuildGraphQLRequestError("APIM Subscription delete request failed.", subscriptionResponse.Error);
                    throw new GraphQLException(error);
                }
                return subscriptionResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in Delete APIM Susbcription Mutation - {ex.Message}", ex);
                throw;
            }
        }




    }
}
