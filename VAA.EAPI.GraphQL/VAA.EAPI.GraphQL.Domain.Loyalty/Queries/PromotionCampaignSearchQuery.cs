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
    public class PromotionCampaignSearchQuery
    {
        private readonly ILogger<PromotionCampaignSearchQuery> _logger;
        public PromotionCampaignSearchQuery(ILogger<PromotionCampaignSearchQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Promotion.Search" })]
        [GraphQLName("loyaltyPromotionCampaignSearch")]
        [GraphQLDescription("Retreive a list of Promotions from reference data")]

        public async Task<List<PromotionCampaignSearchData>> PromotionSearch(string promotionId, [Service] IPromotionCampaignSearchQueryProvider promotionCampaignSearchService, [Service] ILogger<PromotionSearchQuery> logger)
        {
            _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Queries.PromotionCampaignSearchQuery.PromotionSearch() - Start");

            try
            {
                if (promotionCampaignSearchService is null)
                    throw new ArgumentNullException(nameof(promotionCampaignSearchService));
                try
                {
                    return await promotionCampaignSearchService.Query(promotionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error has occured in promotion campaign search query");
                    throw;
                }
            }
            finally
            {
                _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Queries.PromotionCampaignSearchQuery.CampaignType() - End");
            }
        }
    }
}