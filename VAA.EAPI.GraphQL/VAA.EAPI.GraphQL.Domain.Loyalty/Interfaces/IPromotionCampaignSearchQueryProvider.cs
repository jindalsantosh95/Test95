using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IPromotionCampaignSearchQueryProvider
    {
        Task<List<PromotionCampaignSearchData>> Query(string PromotionId);
    }
}