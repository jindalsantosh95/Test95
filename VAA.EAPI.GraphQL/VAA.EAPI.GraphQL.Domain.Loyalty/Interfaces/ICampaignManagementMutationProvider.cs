using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ICampaignManagementMutationProvider
    {
        Task<string> UpdateCampaign(CampaignUpdateMutationRequest campaign);
        Task<string> AssignCampaign(CampaignAssignRequest campaign);
        Task<string> CreateCampaign(CampaignCreateMutationRequest campaign);
        Task<string> LinkCampaignPromotion(CampaignLinkingMutationRequest campaign);
        Task<CampaignUnassignMutationResponse> UnassignCampaign(CampaignUnassignMutationRequest request);
        Task<string> DelinkCampaignPromotion(CampaignLinkingMutationRequest campaignDelinkingMutationRequest);
    }
}
