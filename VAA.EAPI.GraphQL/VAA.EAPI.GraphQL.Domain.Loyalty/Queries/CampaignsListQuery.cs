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
    public class CampaignsListQuery
    {

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.CampaignsList.Read" })]
        [GraphQLName("loyaltyCampaignsList")]
        [GraphQLDescription("Retreive a list of Members enrolled for a given Loyalty Campaigns")]
        public async Task<CampaignsDownloadResponse> CampaignsList([Service] ICampaignsListQueryProvider campaignsListService)
        {
            return await campaignsListService.Query();
        }
    }
}
