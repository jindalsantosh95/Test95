using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class PromotionManagementMutations
    {
        private readonly ILogger<PromotionManagementMutations> _logger;
        private readonly IPromotionManagementMutationProvider _promotionManagementProvider;

        public PromotionManagementMutations(IPromotionManagementMutationProvider promotionManagementProvider, ILogger<PromotionManagementMutations> logger)
        {
            _promotionManagementProvider = promotionManagementProvider ?? throw new ArgumentNullException(nameof(promotionManagementProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = new[] { "Loyalty.Promotion.Update" })]
        [GraphQLName("loyaltyUpdatePromotion")]
        [GraphQLDescription("Update the details of a promotion")]
        public async Task<string> UpdatePromotion(PromotionUpdateMutationRequest promotion)
        {
            _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Mutations.PromotionManagementMutations.UpdatePromotion() - Start");
            try
            {
                try
                {
                    // fire and forget
                    return await _promotionManagementProvider.UpdatePromotion(promotion);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Unable to invoke update promotion ");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unahandled Exception in loyaltyUpdatePromotion mutation");
                    throw;
                }

            }
            finally
            {
                _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Mutations.PromotionManagementMutations.UpdatePromotion() - End");
            }
        }
        [Authorize(Roles = new[] { "Loyalty.Promotion.Create" })]
        [GraphQLName("loyaltyCreatePromotion")]
        [GraphQLDescription("Creates a promotion")]
        public async Task<string> CreatePromotion(PromotionCreateMutationRequest promotion)
        {
            _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Mutations.PromotionManagementMutations.CreatePromotion() - Start");
            try
            {
                try
                {
                    // fire and forget
                    return await _promotionManagementProvider.CreatePromotion(promotion);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Unable to invoke create promotion ");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unahandled Exception in loyaltyCreatePromotion mutation");
                    throw;
                }

            }
            finally
            {
                _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Mutations.PromotionManagementMutations.CreatePromotion() - End");
            }
        }
    }
}
