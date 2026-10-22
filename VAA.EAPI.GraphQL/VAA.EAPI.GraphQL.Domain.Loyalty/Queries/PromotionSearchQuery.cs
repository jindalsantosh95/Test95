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
    public class PromotionSearchQuery
    {
        private readonly ILogger<PromotionSearchQuery> _logger;
        public PromotionSearchQuery(ILogger<PromotionSearchQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Promotion.Search" })]
        [GraphQLName("loyaltyPromotionSearch")]
        [GraphQLDescription("Retreive a list of Promotions from reference data")]

        public async Task<List<PromotionSearchData>> PromotionSearch(string promotionId, string promotionName, string startDateF, string endDateF, string startDateT, string endDateT, string partnerName, string active, [Service] IPromotionSearchQueryProvider promotionSearchService, [Service] ILogger<PromotionSearchQuery> logger)
        {
            _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Queries.PromotionSearchQuery.PromotionSearch() - Start");

            try
            {
                if (promotionSearchService is null)
                    throw new ArgumentNullException(nameof(promotionSearchService));
                try
                {
                    return await promotionSearchService.Query(promotionId, promotionName, startDateF, endDateF, startDateT, endDateT, partnerName, active);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error has occured in promotion search query");
                    throw;
                }
            }
            finally
            {
                _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Queries.CampaignTypeQuery.CampaignType() - End");
            }
        }
    }
}