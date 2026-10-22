using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ICampaignSearchQueryProvider
    {
        Task<List<CampaignSearchData>> Query(CampaignSearchRequest searchRequest);
    }
}
