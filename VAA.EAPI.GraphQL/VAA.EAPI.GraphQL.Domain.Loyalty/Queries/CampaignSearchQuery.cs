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

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]

    public class CampaignSearchQuery
    {
        private readonly ILogger<CampaignSearchQuery> _logger;
        public CampaignSearchQuery(ILogger<CampaignSearchQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseOffsetPaging(IncludeTotalCount = true)]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Campaign.Search" })]
        [GraphQLName("loyaltyCampaignSearch")]
        [GraphQLDescription("Retreive a list of Campaign from reference data")]

        public async Task<List<CampaignSearchData>> CampaignSearch(CampaignSearchRequest campaignSearchRequest, [Service] ICampaignSearchQueryProvider campaignSearchService, [Service] ILogger<CampaignSearchQuery> logger)
        {
            if (campaignSearchService is null)
                throw new ArgumentNullException(nameof(campaignSearchService));

            try
            {
                return await campaignSearchService.Query(campaignSearchRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in campaign search query");
                throw;
            }
        }
    }
}
