using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Campaign;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ValidatePcmEligibilityQuery
    {
        private readonly ICampaignProvider _campaignProvider;
        private readonly ILogger<ValidatePcmEligibilityQuery> _logger;

        public ValidatePcmEligibilityQuery(ICampaignProvider campaignProvider, ILogger<ValidatePcmEligibilityQuery> logger)
        {
            _campaignProvider = campaignProvider ?? throw new ArgumentNullException(nameof(campaignProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("validatePcmEligibility")]
        [GraphQLDescription("Validate Amex promotion unique id eligibility")]
        public async Task<ValidateEligibilityResponse> ValidateEligibility(string uniqueId)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
                throw new ArgumentNullException(nameof(uniqueId));

            try
            {
                return await _campaignProvider.ValidateCampaign(uniqueId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occurred in validate PCM Eligibility campaign query");
                throw;
            }
        }
    }
}