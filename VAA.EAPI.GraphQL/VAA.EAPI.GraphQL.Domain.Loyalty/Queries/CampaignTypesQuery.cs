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
    public class CampaignTypesQuery
    {
        private readonly ILogger<CampaignTypesQuery> _logger;
        public CampaignTypesQuery(ILogger<CampaignTypesQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.CampaignType.Read" })]
        [GraphQLName("loyaltyCampaignTypes")]
        [GraphQLDescription("Retreive a list of Campaign Types from reference data")]

        public async Task<CampaignTypesResponse> CampaignType([Service] ICampaignTypesQueryProvider campaignTypeService, [Service] ILogger<CampaignTypesQuery> logger)
        {
            _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Queries.CampaignTypeQuery.CampaignType() - Start");

            try
            {
                if (campaignTypeService is null)
                    throw new ArgumentNullException(nameof(campaignTypeService));
                try
                {
                    return await campaignTypeService.Query();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error has occured in campaign type query");
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
