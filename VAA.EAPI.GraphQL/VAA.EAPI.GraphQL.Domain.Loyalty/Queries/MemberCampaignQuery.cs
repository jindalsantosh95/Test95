using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class MemberCampaignQuery
    {
        private readonly ILogger<MemberCampaignQuery> _logger;

        public MemberCampaignQuery(ILogger<MemberCampaignQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.MemberCampaign.Read" })]
        [GraphQLName("loyaltyMemberCampaigns")]
        [GraphQLDescription("Retreive a list of Loyalty Campaigns assocaited to the member")]
        public async Task<IEnumerable<MemberCampaign>> MemberCampaign(string customerId, string memberId, [Service] ICustomerProvider customerService, [Service] IMemberCampaignQueryProvider memberCampaignService, [Service] ILogger<MemberCampaignQuery> logger)
        {
            if (memberCampaignService is null)
                throw new ArgumentNullException(nameof(memberCampaignService));

            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberId))
                throw new ArgumentException($"Either '{nameof(customerId)}' or {nameof(memberId)} must be spefied.");

            // If I don't have a customer ID, go get the member based on member ID
            if (string.IsNullOrEmpty(customerId))
                customerId = (await customerService.GetCustomer(customerId, memberId)).Customer.CustomerId;

            return await memberCampaignService.Query(customerId);
        }
    }
}
