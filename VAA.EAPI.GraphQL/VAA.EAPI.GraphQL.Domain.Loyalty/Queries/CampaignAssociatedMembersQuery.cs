using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]

    public class CampaignAssociatedMembersQuery
    {
        private readonly ILogger<CampaignAssociatedMembersQuery> _logger;

        public CampaignAssociatedMembersQuery(ILogger<CampaignAssociatedMembersQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.CampaignAssociatedMembers.Download" })]
        [GraphQLName("loyaltyCampaignAssociatedMembers")]
        [GraphQLDescription("Retreive a list of Members associated to a given Loyalty campaign")]

        public async Task<AssociatedMembersDownloadResponse> AssociatedMembers(string campaignCode, [Service] IAssociatedMembersQueryProvider assocMembersService, [Service] ILogger<CampaignAssociatedMembersQuery> logger)
        {
            if (assocMembersService is null)
                throw new ArgumentNullException(nameof(assocMembersService));

            if (string.IsNullOrEmpty(campaignCode))
                throw new ArgumentException($"{nameof(campaignCode)} must be spefied.");

            try
            {
                return await assocMembersService.Query(campaignCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in AssociatedMembers download query");
                throw;
            }
        }
    }
}
