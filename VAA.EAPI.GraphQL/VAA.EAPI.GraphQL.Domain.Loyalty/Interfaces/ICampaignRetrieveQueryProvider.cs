using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ICampaignRetrieveQueryProvider
    {
        Task<CampaignRetrieveData> Query(string campaignId, string campaignCode);
    }
}
