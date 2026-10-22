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
    public class HouseHoldMutations
    {
        private readonly ILogger<HouseHoldMutations> _logger;
        private readonly IHouseHoldMutationProvider _houseHoldMutationProvider;
        private readonly ICustomerProvider _customerProvider;

        public HouseHoldMutations(ICustomerProvider customerProvider, IHouseHoldMutationProvider houseHoldMutationProvider, ILogger<HouseHoldMutations> logger)
        {
            _houseHoldMutationProvider = houseHoldMutationProvider ?? throw new ArgumentNullException(nameof(houseHoldMutationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [Authorize(Roles = new[] { "Loyalty.Household.Link" })]
        [GraphQLName("loyaltyHouseholdLinking")]
        [GraphQLDescription("Links child details with HOH")]
        public async Task<string> HouseHoldLinking(HouseHoldLinkingMutationRequest houseHoldLinkingRequest, [Service] ICustomerProvider customerService)
        {
            try
            {
                if (string.IsNullOrEmpty(houseHoldLinkingRequest.InitiatedBy)) throw new RequestValidationException($"'{nameof(houseHoldLinkingRequest.InitiatedBy)}'must be specified.");

                var Headcustomer = (await _customerProvider.GetCustomer(houseHoldLinkingRequest.HeadCustomerId, houseHoldLinkingRequest.HeadMemberNumber)).Customer;
                var Childcustomer = (await _customerProvider.GetCustomer(houseHoldLinkingRequest.ChildCustomerId, houseHoldLinkingRequest.ChildMemberNumber)).Customer;

                HouseHoldLinkingRequest houseHoldLinkingReq = new HouseHoldLinkingRequest()
                {
                    InitiatedBy = houseHoldLinkingRequest.InitiatedBy,
                    HeadAccount = new HouseHoldAccount()
                    {
                        CustomerId = string.IsNullOrEmpty(houseHoldLinkingRequest.HeadCustomerId) ? Headcustomer.CustomerId : houseHoldLinkingRequest.HeadCustomerId,
                        MemberId = string.IsNullOrEmpty(houseHoldLinkingRequest.HeadMemberNumber) ? Headcustomer.LoyaltyNumber : houseHoldLinkingRequest.HeadMemberNumber,
                        FirstName = Headcustomer.FirstName,
                        LastName = Headcustomer.LastName,
                        EmailId = Headcustomer.ContactDetails?.EmailDetails?[0]?.emailId?.Trim(),
                    },
                    ChildAccount = new HouseHoldAccount()
                    {
                        CustomerId = string.IsNullOrEmpty(houseHoldLinkingRequest.ChildCustomerId) ? Childcustomer.CustomerId : houseHoldLinkingRequest.ChildCustomerId,
                        MemberId = string.IsNullOrEmpty(houseHoldLinkingRequest.ChildMemberNumber) ? Childcustomer.LoyaltyNumber : houseHoldLinkingRequest.ChildMemberNumber,
                        FirstName = Childcustomer.FirstName,
                        LastName = Childcustomer.LastName,
                        EmailId = Childcustomer.ContactDetails?.EmailDetails?[0]?.emailId?.Trim(),
                    }
                };

                return await _houseHoldMutationProvider.HouseHoldLinking(houseHoldLinkingReq);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke Household Linking ");

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in loyaltyHouseholdLinking mutation");

                throw;
            }
        }

        [Authorize(Roles = new[] { "Loyalty.Household.Delink" })]
        [GraphQLName("loyaltyHouseholdDelinking")]
        [GraphQLDescription("Delinks child details with HOH")]
        public async Task<string> HouseHoldDelinking(HouseHoldDelinkingMutationRequest houseHoldDelinkingRequest, [Service] ICustomerProvider customerService)
        {
            try
            {
                if (string.IsNullOrEmpty(houseHoldDelinkingRequest.InitiatedBy)) throw new RequestValidationException($"'{nameof(houseHoldDelinkingRequest.InitiatedBy)}'must be specified.");

                var Headcustomer = (await _customerProvider.GetCustomer(houseHoldDelinkingRequest.HeadCustomerId, houseHoldDelinkingRequest.HeadMemberNumber)).Customer;
                var Childcustomer = (await _customerProvider.GetCustomer(houseHoldDelinkingRequest.ChildCustomerId, houseHoldDelinkingRequest.ChildMemberNumber)).Customer;

                HouseHoldDelinkingRequest houseHoldDelinkingReq = new HouseHoldDelinkingRequest()
                {
                    InitiatedBy = houseHoldDelinkingRequest.InitiatedBy,
                    HeadAccount = new HouseHoldAccount()
                    {
                        CustomerId = string.IsNullOrEmpty(houseHoldDelinkingRequest.HeadCustomerId) ? Headcustomer.CustomerId : houseHoldDelinkingRequest.HeadCustomerId,
                        MemberId = string.IsNullOrEmpty(houseHoldDelinkingRequest.HeadMemberNumber) ? Headcustomer.LoyaltyNumber : houseHoldDelinkingRequest.HeadMemberNumber,
                        FirstName = Headcustomer.FirstName,
                        LastName = Headcustomer.LastName,
                        EmailId = Headcustomer.ContactDetails?.EmailDetails?[0]?.emailId?.Trim()
                    },
                    ChildAccount = new HouseHoldAccount()
                    {
                        CustomerId = string.IsNullOrEmpty(houseHoldDelinkingRequest.ChildCustomerId) ? Childcustomer.CustomerId : houseHoldDelinkingRequest.ChildCustomerId,
                        MemberId = string.IsNullOrEmpty(houseHoldDelinkingRequest.ChildMemberNumber) ? Childcustomer.LoyaltyNumber : houseHoldDelinkingRequest.ChildMemberNumber,
                        FirstName = Childcustomer.FirstName,
                        LastName = Childcustomer.LastName,
                        EmailId = Childcustomer.ContactDetails?.EmailDetails?[0]?.emailId?.Trim()
                    }
                };

                return await _houseHoldMutationProvider.HouseHoldDelinking(houseHoldDelinkingReq);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke Household Delinking process");

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in loyaltyHouseholdDelinking mutation");

                throw;
            }
        }
    }
}
