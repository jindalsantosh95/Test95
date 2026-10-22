using VAA.EAPI.GraphQL.Domain.Account.Models.Campaign;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface ICampaignProvider
    {
        Task<ValidateEligibilityResponse> ValidateCampaign(string uniqueId);
        Task<string> AssignCampaign(AssignCampaignRequest assignCampaignRequest);
        Task<string> UpdateCampaign(UpdateCampaignRequest updateCampaignRequest);
    }
}