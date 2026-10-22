using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface
{
    public interface IReportingDBServices
    {
        Task<int> GetAssociatedMemberCount(string campaignCode);

        Task<List<AssociatedMemberData>> GetAssociatedMembersData(string campaignCode);
    }
}
