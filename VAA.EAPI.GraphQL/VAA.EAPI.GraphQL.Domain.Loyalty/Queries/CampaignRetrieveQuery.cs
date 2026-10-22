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

    public class CampaignRetrieveQuery
    {
        private readonly ILogger<CampaignRetrieveQuery> _logger;

        public CampaignRetrieveQuery(ILogger<CampaignRetrieveQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Campaign.Retrieve" })]
        [GraphQLName("loyaltyCampaignRetrieve")]
        [GraphQLDescription("Retreive Campaign details from reference data for the given campaign")]

        public async Task<CampaignRetrieveData> CampaignRetrieve(string campaignId, string campaignCode, [Service] ICampaignRetrieveQueryProvider campaignRetrieveService, [Service] ILogger<CampaignRetrieveQuery> logger)
        {
            if (campaignRetrieveService is null)
                throw new ArgumentNullException(nameof(campaignRetrieveService));

            if (string.IsNullOrEmpty(campaignId) && string.IsNullOrEmpty(campaignCode))
                throw new ArgumentException($"Either '{nameof(campaignId)}' or {nameof(campaignCode)} must be spefied.");

            try
            {
                return await campaignRetrieveService.Query(campaignId, campaignCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in campaign retrieve query");
                throw;
            }
        }
    }
}
