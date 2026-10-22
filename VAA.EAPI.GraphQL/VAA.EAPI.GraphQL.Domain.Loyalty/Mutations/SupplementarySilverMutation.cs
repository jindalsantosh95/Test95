using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class SupplementarySilverMutation
    {
        private readonly ILogger<SupplementarySilverMutation> _logger;
        private readonly ISupplementarySilverMutationProvider _supplementarySilverMutationProvider;
        private readonly ICustomerProvider _customerProvider;

        public SupplementarySilverMutation(ICustomerProvider customerProvider, ISupplementarySilverMutationProvider supplementarySilverMutationProvider, ILogger<SupplementarySilverMutation> logger)
        {
            _supplementarySilverMutationProvider = supplementarySilverMutationProvider ?? throw new ArgumentNullException(nameof(supplementarySilverMutationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [Authorize(Roles = new[] { "Loyalty.SupplementarySilver.Delink" })]
        [GraphQLName("loyaltySupplementarySilverDelinking")]
        [GraphQLDescription("Delinks child details with SSA")]
        public async Task<string> SupplementarySilverDelinking(SupplementarySilverDelinkingMutationRequest supplementarySilverDelinkingRequest, [Service] ICustomerProvider customerService)
        {
            try
            {
                if (string.IsNullOrEmpty(supplementarySilverDelinkingRequest.InitiatedBy)) throw new RequestValidationException($"'{nameof(SupplementarySilverDelinkingRequest.InitiatedBy)}'must be specified.");

                var Headcustomer = (await _customerProvider.GetCustomer(supplementarySilverDelinkingRequest.HeadCustomerId, supplementarySilverDelinkingRequest.HeadMemberNumber)).Customer;
                var Childcustomer = (await _customerProvider.GetCustomer(supplementarySilverDelinkingRequest.ChildCustomerId, supplementarySilverDelinkingRequest.ChildMemberNumber)).Customer;

                SupplementarySilverDelinkingRequest supplementarySilverDelinkingReq = new SupplementarySilverDelinkingRequest()
                {
                    InitiatedBy = supplementarySilverDelinkingRequest.InitiatedBy,
                    HeadAccount = new HouseHoldAccount()
                    {
                        CustomerId = string.IsNullOrEmpty(supplementarySilverDelinkingRequest.HeadCustomerId) ? Headcustomer.CustomerId : supplementarySilverDelinkingRequest.HeadCustomerId,
                        MemberId = string.IsNullOrEmpty(supplementarySilverDelinkingRequest.HeadMemberNumber) ? Headcustomer.LoyaltyNumber : supplementarySilverDelinkingRequest.HeadMemberNumber
                    },
                    ChildAccount = new HouseHoldAccount()
                    {
                        CustomerId = string.IsNullOrEmpty(supplementarySilverDelinkingRequest.ChildCustomerId) ? Childcustomer.CustomerId : supplementarySilverDelinkingRequest.ChildCustomerId,
                        MemberId = string.IsNullOrEmpty(supplementarySilverDelinkingRequest.ChildMemberNumber) ? Childcustomer.LoyaltyNumber : supplementarySilverDelinkingRequest.ChildMemberNumber
                    }
                };

                return await _supplementarySilverMutationProvider.SupplementarySilverDelinking(supplementarySilverDelinkingReq);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke Supplementary Silver Delinking process");

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in Supplementary Silver Delinking mutation");

                throw;
            }
        }

        [Authorize(Roles = new[] { "Loyalty.SupplementarySilver.Link" })]
        [GraphQLName("loyaltySupplementarySilverLinking")]
        [GraphQLDescription("SupplementarySilverLinking - Links child details with head gold account ")]
        public async Task<string> SupplementarySilverLinking(SupplementarySilverLinkingRequest supplementarySilverLinkingRequest,
            [Service] ICustomerProvider customerService)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(supplementarySilverLinkingRequest.InitiatedBy))
                    throw new RequestValidationException($"'{nameof(supplementarySilverLinkingRequest.InitiatedBy)}'must be specified.");

                var headCustomer = (await _customerProvider.GetCustomer(supplementarySilverLinkingRequest.HeadCustomerId, supplementarySilverLinkingRequest.HeadMemberNumber)).Customer;
                var childCustomer = (await _customerProvider.GetCustomer(supplementarySilverLinkingRequest.ChildCustomerId, supplementarySilverLinkingRequest.ChildMemberNumber)).Customer;

                SupplementarySilverDelinkingRequest supplementarySilverDelinkingReq = new SupplementarySilverDelinkingRequest()
                {
                    InitiatedBy = supplementarySilverLinkingRequest.InitiatedBy,
                    HeadAccount = new HouseHoldAccount()
                    {
                        CustomerId = string.IsNullOrEmpty(supplementarySilverLinkingRequest.HeadCustomerId) ? headCustomer.CustomerId : supplementarySilverLinkingRequest.HeadCustomerId,
                        MemberId = string.IsNullOrEmpty(supplementarySilverLinkingRequest.HeadMemberNumber) ? headCustomer.LoyaltyNumber : supplementarySilverLinkingRequest.HeadMemberNumber
                    },
                    ChildAccount = new HouseHoldAccount()
                    {
                        CustomerId = string.IsNullOrEmpty(supplementarySilverLinkingRequest.ChildCustomerId) ? childCustomer.CustomerId : supplementarySilverLinkingRequest.ChildCustomerId,
                        MemberId = string.IsNullOrEmpty(supplementarySilverLinkingRequest.ChildMemberNumber) ? childCustomer.LoyaltyNumber : supplementarySilverLinkingRequest.ChildMemberNumber
                    }
                };

                return await _supplementarySilverMutationProvider.SupplementarySilverLinking(supplementarySilverLinkingRequest);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke Supplementary Silver Linking ");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in Supplementary Silver Linking mutation");
                throw;
            }
        }
    }
}
