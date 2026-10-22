using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CampaignRetrieve.Interface
{
    public interface ICampaignRetrieveRepository
    {
        Task<Result<Response<MemberCampaignResponse>>> GetMemberCampaigns(string customerId);
    }
}
