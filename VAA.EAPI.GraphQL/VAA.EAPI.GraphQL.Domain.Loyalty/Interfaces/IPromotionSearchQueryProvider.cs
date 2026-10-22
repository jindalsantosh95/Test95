using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IPromotionSearchQueryProvider
    {
        Task<List<PromotionSearchData>> Query(string PromotionId, string PromotionName, string StartDateF, string EndDateF, string StartDateT, string EndDateT, string PartnerName, string Active);
    }
}