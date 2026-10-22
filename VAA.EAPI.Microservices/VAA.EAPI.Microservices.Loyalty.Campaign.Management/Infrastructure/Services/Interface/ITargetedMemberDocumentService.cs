using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface
{
    public interface ITargetedMemberDocumentService
    {
        public Task<TargetedMembershipResponse> GetMemberCampaigns(string customerId);
    }
}
