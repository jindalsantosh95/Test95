using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class PromotionsListQuery
    {
        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.PromotionsList.Read" })]
        [GraphQLName("loyaltyPromotionsList")]
        [GraphQLDescription("Retreive a list of Members enrolled for a given Loyalty promotion")]
        public async Task<PromotionsDownloadResponse> PromotionsList([Service] IPromotionsListQueryProvider promotionsListService)
        {
            return await promotionsListService.Query();
        }
    }
}

