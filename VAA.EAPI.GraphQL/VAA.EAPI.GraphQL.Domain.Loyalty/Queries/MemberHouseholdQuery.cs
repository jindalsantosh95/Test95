using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class MemberHouseholdQuery
    {
        private readonly ILogger<MemberHouseholdQuery> _logger;

        public MemberHouseholdQuery(ILogger<MemberHouseholdQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Household.Read" })]
        [GraphQLName("loyaltyMemberHousehold")]
        [GraphQLDescription("Retreive household account details for given member")]
        public async Task<MemberHouseholdResponse> MemberHousehold(string customerId, string memberId, [Service] ICustomerProvider customerService, [Service] IMemberHouseholdQueryProvider memberHouseholdService, [Service] ILogger<MemberHouseholdQuery> logger)
        {
            if (memberHouseholdService is null)
                throw new ArgumentNullException(nameof(memberHouseholdService));

            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberId))
                throw new ArgumentException($"Either '{nameof(customerId)}' or {nameof(memberId)} must be spefied.");

            // If I don't have a customer ID, go get the member based on member ID
            if (string.IsNullOrEmpty(customerId))
                customerId = (await customerService.GetCustomer(customerId, memberId)).Customer.CustomerId;

            MemberHouseholdResponse response = await memberHouseholdService.Query(customerId);

            if (!String.IsNullOrEmpty(response?.HeadOfHousehold?.CustomerId))
            {
                var customer = (await customerService.GetCustomer(response.HeadOfHousehold.CustomerId, memberId)).Customer;
                response.HeadOfHousehold.MemberNumber = customer.LoyaltyNumber;
                response.HeadOfHousehold.FirstName = customer.FirstName;
                response.HeadOfHousehold.LastName = customer.LastName;
                response.HeadOfHousehold.MemberStatus = customer.Status;
            }

            if (response?.HouseholdMembers != null)
            {
                foreach (HouseholdMember householdMember in response.HouseholdMembers)
                {
                    if (!String.IsNullOrEmpty(householdMember.CustomerId))
                    {
                        var customer = (await customerService.GetCustomer(householdMember.CustomerId, memberId)).Customer;
                        householdMember.MemberNumber = customer.LoyaltyNumber;
                        householdMember.FirstName = customer.FirstName;
                        householdMember.LastName = customer.LastName;
                        householdMember.MemberStatus = customer.Status;
                    }
                }
            }

            if (response?.HouseholdHistory != null)
            {
                foreach (HouseholdHistoryItem householdHistoryItem in response.HouseholdHistory)
                {
                    if (!String.IsNullOrEmpty(householdHistoryItem.CustomerId))
                    {
                        var customer = (await customerService.GetCustomer(householdHistoryItem.CustomerId, memberId)).Customer;
                        householdHistoryItem.MemberNumber = customer.LoyaltyNumber;
                        householdHistoryItem.FirstName = customer.FirstName;
                        householdHistoryItem.LastName = customer.LastName;
                        householdHistoryItem.MemberStatus = customer.Status;
                    }
                }
            }
            return response;
        }
    }
}

