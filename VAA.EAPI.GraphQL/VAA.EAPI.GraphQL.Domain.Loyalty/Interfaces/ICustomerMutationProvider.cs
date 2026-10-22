using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ICustomerMutationProvider
    {
        Task<TopMemberAwardResponse> AssignTopMember(string customerId);
        Task<string> RegisterCampaign(CampaignRegisterRequest request);
        Task<string> UpgradeVACMember(UpgradeMemberMSRequest request);
    }
}
