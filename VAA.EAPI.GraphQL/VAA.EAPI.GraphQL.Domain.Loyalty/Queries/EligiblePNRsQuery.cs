using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.StampFCNumToPNR;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class EligiblePNRsQuery
    {
        private readonly IEligiblePNRsQueryProvider _queryProvider;
        private readonly ICustomerProvider _customerProvider;

        public EligiblePNRsQuery(IEligiblePNRsQueryProvider queryProvider, ICustomerProvider customerProvider)
        {
            _queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.EligiblePNRs.Read" })]
        [GraphQLName("loyaltyEligiblePNRs")]
        [GraphQLDescription("Retreive a list of eligible PNRs for Auto-Stamping FC Number")]
        public async Task<IEnumerable<FCNumToPNRDetails>> RetrieveEligiblePNRs(GetEligiblePNRsRequest getEligiblePNRsRequest)
        {
            if (string.IsNullOrEmpty(getEligiblePNRsRequest.MemberNumber) && string.IsNullOrEmpty(getEligiblePNRsRequest.CustomerId))
                throw new ArgumentException($"Either '{nameof(getEligiblePNRsRequest.MemberNumber)}' or {nameof(getEligiblePNRsRequest.CustomerId)} is mandatory.");

            if (string.IsNullOrEmpty(getEligiblePNRsRequest.MemberNumber) || string.IsNullOrEmpty(getEligiblePNRsRequest.CustomerDetails.FirstName) || string.IsNullOrEmpty(getEligiblePNRsRequest.CustomerDetails.LastName) || string.IsNullOrEmpty(getEligiblePNRsRequest.CustomerDetails.ContactDetails?.EmailDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.EmailId))
            {
                CustomerCustMS customerResponse = (await _customerProvider.GetCustomer(getEligiblePNRsRequest.CustomerId, getEligiblePNRsRequest.MemberNumber)).Customer;

                getEligiblePNRsRequest.MemberNumber = string.IsNullOrEmpty(getEligiblePNRsRequest.MemberNumber) ? customerResponse.LoyaltyNumber : getEligiblePNRsRequest.MemberNumber;
                getEligiblePNRsRequest.CustomerDetails.FirstName = string.IsNullOrEmpty(getEligiblePNRsRequest.CustomerDetails.FirstName) ? customerResponse.FirstName : getEligiblePNRsRequest.CustomerDetails.FirstName;
                getEligiblePNRsRequest.CustomerDetails.LastName = string.IsNullOrEmpty(getEligiblePNRsRequest.CustomerDetails.LastName) ? customerResponse.LastName : getEligiblePNRsRequest.CustomerDetails.LastName;
                getEligiblePNRsRequest.CustomerDetails.MiddleName = customerResponse.MiddleName;
                getEligiblePNRsRequest.CustomerDetails.ContactDetails.EmailDetails = string.IsNullOrEmpty(getEligiblePNRsRequest.CustomerDetails.ContactDetails?.EmailDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.EmailId) ? [new()
                {
                    EmailId = customerResponse.ContactDetails.EmailDetails.FirstOrDefault(x => x.PreferredFlag == "Y")?.emailId,
                    PreferredFlag = customerResponse.ContactDetails.EmailDetails.FirstOrDefault(x => x.PreferredFlag == "Y")?.PreferredFlag
                } ] : getEligiblePNRsRequest.CustomerDetails.ContactDetails?.EmailDetails;
            }

            return await _queryProvider.GetEligiblePNRs(getEligiblePNRsRequest);

        }
    }
}
