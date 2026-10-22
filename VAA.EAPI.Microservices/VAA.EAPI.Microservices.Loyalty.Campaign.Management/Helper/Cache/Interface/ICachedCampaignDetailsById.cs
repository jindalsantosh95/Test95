using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper.Cache.Interface
{
    public interface ICachedCampaignDetailsById
    {
        Task<CampaignDetails> GetCachedCampaignDetailsById(string campaignId);
    }
}
