using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Mappers;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class CustomerMutation
    {
        private readonly ICustomerMutationProvider _customerMutationProvider;
        private readonly ILogger<ICustomerLogger> _logger;
        private readonly ICustomerProvider _customerProvider;

        public CustomerMutation(ICustomerProvider customerProvider, ICustomerMutationProvider customerMutationProvider, ILogger<ICustomerLogger> logger)
        {
            _customerMutationProvider = customerMutationProvider ?? throw new ArgumentNullException(nameof(customerMutationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider;
        }

        [GraphQLName("loyaltyAssignTopMember")]
        [Authorize(Roles = new[] { "TopMember.Assign" })]
        [GraphQLDescription("Assign 'Top Member' status to the customer")]
        public async Task<Award> AssignTopMember(string customerId, string memberNumber)
        {
            try
            {
                Infrastructure.Entities.CustomerMS.CustomerCustMS cust = (await _customerProvider.GetCustomer(customerId, memberNumber)).Customer;
                // Check Customer exists, by retrieving it
                var tmResponse = await _customerMutationProvider.AssignTopMember(cust.CustomerId);

                if (tmResponse.Errors is not null)
                    throw new GraphQLException(tmResponse.Errors.Select(err => ErrorBuilder.New()
                                                                                        .SetMessage(Json.Serialize(err))
                                                                                        .Build()).ToArray());
                return tmResponse.Award;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to retrieve Customer details");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in Customer query");
                throw;
            }

        }

        [GraphQLName("loyaltyRegisterCampaign")]
        [Authorize(Roles = new[] { "Loyalty.RegisterCampaign" })]
        [GraphQLDescription("Register a customer to a campaign")]
        public async Task<RegisterCampaignResponse> RegisterCampaign(CampaignRegisterMutationRequest campaignRegisterRequest)
        {
            try
            {
                if (campaignRegisterRequest is null) throw new ArgumentNullException(nameof(campaignRegisterRequest));

                var customer = (await _customerProvider.GetCustomer(campaignRegisterRequest.CustomerId, campaignRegisterRequest.MemberNumber)).Customer;
                _logger.LogDebug($"Customer profile data fetched for customer id :{customer.CustomerId}");
                var response = await _customerMutationProvider.RegisterCampaign(CampaignRegisterMapper.MapToCampaignRegister(campaignRegisterRequest, customer));
                return new()
                {
                    Result = true,
                    Message = response
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to retrieve Customer details");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in Customer query");
                throw;
            }
        }
    }
}
