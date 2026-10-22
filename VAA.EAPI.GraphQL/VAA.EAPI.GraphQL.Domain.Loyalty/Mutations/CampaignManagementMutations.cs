using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class CampaignManagementMutations
    {
        private readonly ILogger<CampaignManagementMutations> _logger;
        private readonly ICampaignManagementMutationProvider _campaignManagementProvider;
        private readonly ITargetedMembershipQueryProvider _targetedMembershipProvider;

        public CampaignManagementMutations(ICampaignManagementMutationProvider campaignManagementProvider, ILogger<CampaignManagementMutations> logger, ITargetedMembershipQueryProvider targetedMembershipProvider)
        {
            _campaignManagementProvider = campaignManagementProvider ?? throw new ArgumentNullException(nameof(campaignManagementProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _targetedMembershipProvider = targetedMembershipProvider ?? throw new ArgumentNullException(nameof(targetedMembershipProvider));
        }

        [Authorize(Roles = new[] { "Loyalty.UpdateCampaign" })]
        [GraphQLName("loyaltyUpdateCampaign")]
        [GraphQLDescription("Update the details of a campaign")]
        public async Task<string> UpdateCampaign(CampaignUpdateMutationRequest campaign)
        {
            try
            {
                // fire and forget
                return await _campaignManagementProvider.UpdateCampaign(campaign);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke update campaign ");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in loyaltyUpdateCampaign mutation");
                throw;
            }

        }

        [Authorize(Roles = new[] { "Loyalty.AssignCampaign" })]
        [GraphQLName("loyaltyAssignCampaign")]
        [GraphQLDescription("Assign the details of a campaign")]
        public async Task<string> AssignCampaign(CampaignAssignMutationRequest campaignAssignRequest, [Service] ICustomerProvider customerService, [Service] ITargetedMembershipQueryProvider targetedMembershipService)
        {
            try
            {
                if (campaignAssignRequest is null) throw new ArgumentNullException(nameof(campaignAssignRequest));
                string customerId = campaignAssignRequest.CustomerId;
                string memberNumber = campaignAssignRequest.MemberNumber;

                if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                    throw new ArgumentException($"Either '{nameof(customerId)}' or {nameof(memberNumber)} must be spefied.");

                if (!string.IsNullOrEmpty(customerId))
                {
                    if ((bool)(await _targetedMembershipProvider.Query(customerId))?.Where(x => x.ProfilingCampaignSourceId == campaignAssignRequest.CampaignId).Any())
                        throw new RequestValidationException("Campaign Code Already Assigned");
                }

                var customer = (await customerService.GetCustomer(customerId, memberNumber)).Customer;
                _logger.LogDebug($"Customer profile data fetched for customer id :{customer.CustomerId}");

                if (string.IsNullOrEmpty(customerId))
                {
                    if ((bool)(await _targetedMembershipProvider.Query(customer.CustomerId))?.Where(x => x.ProfilingCampaignSourceId == campaignAssignRequest.CampaignId).Any())
                        throw new RequestValidationException("Campaign Code Already Assigned");
                }

                CampaignAssignRequest campaignAssignReq = CampaignAssignMapper.MapToCampaignAssign(campaignAssignRequest, customer);
                return await _campaignManagementProvider.AssignCampaign(campaignAssignReq);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke Assign campaign ");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in loyaltyAssignCampaign mutation");
                throw;
            }

        }

        [Authorize(Roles = new[] { "Loyalty.CreateCampaign" })]
        [GraphQLName("loyaltyCreateCampaign")]
        [GraphQLDescription("Creates a campaign")]
        public async Task<string> CreateCampaign(CampaignCreateMutationRequest campaign)
        {
            try
            {
                // fire and forget
                return await _campaignManagementProvider.CreateCampaign(campaign);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke create campaign ");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in loyaltyCreateCampaign mutation");
                throw;
            }

        }

        [Authorize(Roles = new[] { "Loyalty.Campaign.Unassign" })]
        [GraphQLName("loyaltyUnassignCampaign")]
        [GraphQLDescription("Unassign the details of a campaign from a FC member")]
        public async Task<string> UnassignCampaign(CampaignUnassignMutationRequest campaignUnassignRequest)
        {
            _logger.LogInformation("CampaignManagementMutations - UnassignCampaign process started");
            try
            {
                if (campaignUnassignRequest is null) throw new ArgumentNullException(nameof(campaignUnassignRequest));

                if (string.IsNullOrWhiteSpace(campaignUnassignRequest.CustomerId) ||
                    string.IsNullOrWhiteSpace(campaignUnassignRequest.DocumentId))
                    throw new ArgumentException($"'{nameof(campaignUnassignRequest.CustomerId)}' And {nameof(campaignUnassignRequest.DocumentId)} must be spefied.");

                var response = await _campaignManagementProvider.UnassignCampaign(campaignUnassignRequest);

                // If there were any errors, then report them to consumer, 
                if (response.Errors is not null)
                {
                    var builder = ErrorBuilder.New()
                                              .SetMessage("Un-assign Campaign mutation request failed.");
                    response.Errors.ForEach(err =>
                    {
                        builder.SetExtension("code", err.Code);
                        builder.SetExtension("message", err.Message);
                        builder.SetExtension("source", err.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                return response.Value;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke Assign campaign ");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in loyaltyUnassignCampaign mutation");
                throw;
            }
            finally
            {
                _logger.LogInformation("CampaignManagementMutations - UnassignCampaign process ended");
            }
        }

        [Authorize(Roles = new[] { "Loyalty.Promotion.Link" })]
        [GraphQLName("loyaltyLinkCampaignPromotion")]
        [GraphQLDescription("Link the details of a campaign and promotion")]
        public async Task<string> LinkCampaignPromotion(CampaignLinkingMutationRequest linkCampaignPromotionRequest)
        {
            _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Mutations.CampaignManagementMutations.LinkCampaignPromotion() - Start");
            try
            {
                try
                {
                    // fire and forget
                    return await _campaignManagementProvider.LinkCampaignPromotion(linkCampaignPromotionRequest);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Unable to invoke link campaign and promotion ");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unahandled Exception in loyaltyLinkCampaignPromotion mutation");
                    throw;
                }
            }
            finally
            {
                _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Mutations.CampaignManagementMutations.LinkCampaignPromotion() - End");
            }
        }

        [Authorize(Roles = new[] { "Loyalty.Promotion.Delink" })]
        [GraphQLName("loyaltyDelinkCampaignPromotion")]
        [GraphQLDescription("Delink the details of a campaign and promotion")]
        public async Task<string> DelinkCampaignPromotion(CampaignLinkingMutationRequest delinkCampaignPromotionRequest)
        {
            try
            {
                // fire and forget
                return await _campaignManagementProvider.DelinkCampaignPromotion(delinkCampaignPromotionRequest);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke delink campaign and promotion ");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in loyaltyDelinkCampaignPromotion mutation");
                throw;
            }
        }
    }
}
