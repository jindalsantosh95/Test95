using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IMemberCampaignQueryProvider
    {
        Task<IEnumerable<MemberCampaign>> Query(string customerId);
    }
}
