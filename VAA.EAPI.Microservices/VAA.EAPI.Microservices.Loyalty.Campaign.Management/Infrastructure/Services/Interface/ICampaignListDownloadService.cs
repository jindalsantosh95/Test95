using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface
{
    public interface ICampaignListDownloadService
    {
        public Task<string> CreateXlStatementAndUploadToBlob(CampaignSearchResponse campaignList);
    }
}
